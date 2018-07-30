using System;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AzureStorage.Tables;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.ApiLibrary.Middleware;
using Lykke.Common.ApiLibrary.Swagger;
using Lykke.Job.CandlesProducer.Core.Domain;
using Lykke.Job.CandlesProducer.Core.Domain.Candles;
using Lykke.Job.CandlesProducer.Core.Services;
using Lykke.Job.CandlesProducer.Models;
using Lykke.Job.CandlesProducer.Modules;
using Lykke.Job.CandlesProducer.Settings;
using Lykke.Logs;
using Lykke.Logs.MsSql;
using Lykke.Logs.Slack;
using Lykke.SettingsReader;
using Lykke.SlackNotification.AzureQueue;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Job.CandlesProducer
{
    [UsedImplicitly]
    public class Startup
    {
        private IContainer ApplicationContainer { get; set; }
        private IConfigurationRoot Configuration { get; }
        private ILog Log { get; set; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        [UsedImplicitly]
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.ContractResolver =
                        new Newtonsoft.Json.Serialization.DefaultContractResolver();
                });

            services.AddSwaggerGen(options =>
            {
                options.DefaultLykkeConfiguration("v1", "CandlesProducer API");
            });

            var builder = new ContainerBuilder();
            var appSettings = Configuration.LoadSettings<AppSettings>();
            var quotesSourceType = appSettings.CurrentValue.CandlesProducerJob != null ? QuotesSourceType.Spot : QuotesSourceType.Mt;
            var jobSettings = quotesSourceType == QuotesSourceType.Spot 
                ? appSettings.Nested(x => x.CandlesProducerJob) 
                : appSettings.Nested(x => x.MtCandlesProducerJob);

            if (jobSettings.CurrentValue.Db.StorageMode == StorageMode.Azure)
            {
                Log = CreateLogWithSlack(
                    services,
                    appSettings.CurrentValue.SlackNotifications,
                    jobSettings.ConnectionString(x => x.Db.LogsConnString),
                    jobSettings.CurrentValue.Db.StorageMode);
            }
            else if (jobSettings.CurrentValue.Db.StorageMode == StorageMode.SqlServer)
            {
                Log = CreateLogWithSlack(
                    services,
                    appSettings.CurrentValue.SlackNotifications,
                    jobSettings.ConnectionString(x => x.Db.LogsConnString),
                    jobSettings.CurrentValue.Db.StorageMode);
            }

           
            
            builder.RegisterModule(new JobModule(
                jobSettings.CurrentValue, 
                jobSettings.Nested(x => x.Db), 
                appSettings.CurrentValue.Assets,
                quotesSourceType, Log));

            builder.Populate(services);

            ApplicationContainer = builder.Build();

            return new AutofacServiceProvider(ApplicationContainer);
        }

        [UsedImplicitly]
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime appLifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseLykkeMiddleware("CandlesProducer", ex => new ErrorResponse { ErrorMessage = "Technical problem" });

            app.UseMvc();
            app.UseSwagger();
            app.UseSwaggerUi();
            app.UseStaticFiles();

            appLifetime.ApplicationStarted.Register(() => StartApplication().Wait());
            appLifetime.ApplicationStopping.Register(() => StopApplication().Wait());
            appLifetime.ApplicationStopped.Register(() => CleanUp().Wait());
        }

        private async Task StartApplication()
        {
            try
            {
                var startupManager = ApplicationContainer.Resolve<IStartupManager>();

                await startupManager.StartAsync();

                await Log.WriteMonitorAsync("", "", "Started");
            }
            catch (Exception ex)
            {
                await Log.WriteFatalErrorAsync(nameof(Startup), nameof(StartApplication), "", ex);
            }
        }

        private async Task StopApplication()
        {
            try
            {
                var shutdownManager = ApplicationContainer.Resolve<IShutdownManager>();

                await shutdownManager.ShutdownAsync();
            }
            catch (Exception ex)
            {
                if (Log != null)
                {
                    await Log.WriteFatalErrorAsync(nameof(Startup), nameof(StopApplication), "", ex);
                }
            }
        }

        private async Task CleanUp()
        {
            try
            {
                if (Log != null)
                {
                    await Log.WriteMonitorAsync("", "", "Terminating");
                }

                ApplicationContainer.Dispose();
            }
            catch (Exception ex)
            {
                if (Log != null)
                {
                    await Log.WriteFatalErrorAsync(nameof(Startup), nameof(CleanUp), "", ex);
                    (Log as IDisposable)?.Dispose();
                }
            }
        }

        private static ILog CreateLogWithSlack(IServiceCollection services, SlackNotificationsSettings slackSettings, IReloadingManager<string> dbLogConnectionStringManager, StorageMode smode)
        {            
            var consoleLogger = new LogToConsole();
            var aggregateLogger = new AggregateLogger();

            aggregateLogger.AddLog(consoleLogger);

            LykkeLogToAzureSlackNotificationsManager slackNotificationsManager = null;
            if (slackSettings != null)
            {
                // Creating slack notification service, which logs own azure queue processing messages to aggregate log
                var slackService = services.UseSlackNotificationsSenderViaAzureQueue(new AzureQueueIntegration.AzureQueueSettings
                {
                    ConnectionString = slackSettings.AzureQueue.ConnectionString,
                    QueueName = slackSettings.AzureQueue.QueueName
                }, aggregateLogger);

                slackNotificationsManager = new LykkeLogToAzureSlackNotificationsManager(slackService, consoleLogger);
                var logToSlack = LykkeLogToSlack.Create(slackService, "Prices");

                aggregateLogger.AddLog(logToSlack);
            }

            if (smode == StorageMode.SqlServer)
            {
                var sqlLogger = new LogToSql(new LogMsSql("CandlesProducerServiceLog",
                    dbLogConnectionStringManager.CurrentValue));

                aggregateLogger.AddLog(sqlLogger);
            }
            else if (smode == StorageMode.Azure)
            {
                var dbLogConnectionString = dbLogConnectionStringManager.CurrentValue;

                // Creating azure storage logger, which logs own messages to concole log
                if (!string.IsNullOrEmpty(dbLogConnectionString) && !(dbLogConnectionString.StartsWith("${") && dbLogConnectionString.EndsWith("}")))
                {
                    var persistenceManager = new LykkeLogToAzureStoragePersistenceManager(
                        AzureTableStorage<LogEntity>.Create(dbLogConnectionStringManager, "CandlesProducerLog", consoleLogger),
                        consoleLogger);

                    var azureStorageLogger = new LykkeLogToAzureStorage(
                        persistenceManager,
                        slackNotificationsManager,
                        consoleLogger);

                    azureStorageLogger.Start();

                    aggregateLogger.AddLog(azureStorageLogger);
                }
            }

            return aggregateLogger;
        }
    }
}
