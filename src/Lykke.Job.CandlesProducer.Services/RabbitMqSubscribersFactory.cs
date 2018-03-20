using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Job.CandlesProducer.Core.Services;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;

namespace Lykke.Job.CandlesProducer.Services
{
    [UsedImplicitly]
    public class RabbitMqSubscribersFactory : IRabbitMqSubscribersFactory
    {
        private readonly ILog _log;

        public RabbitMqSubscribersFactory(ILog log)
        {
            _log = log;
        }

        public IStopable Create<TMessage>(string connectionString, string @namespace, string source, Func<TMessage, Task> handler, string queueSuffix = null)
        {
            var settings = RabbitMqSubscriptionSettings
                .CreateForSubscriber(connectionString, @namespace, source, @namespace, $"candlesproducer{queueSuffix}")
                .MakeDurable();

            return new RabbitMqSubscriber<TMessage>(settings,
                    new ResilientErrorHandlingStrategy(_log, settings,
                        retryTimeout: TimeSpan.FromSeconds(10),
                        retryNum: 10,
                        next: new DeadQueueErrorHandlingStrategy(_log, settings)))
                .SetMessageDeserializer(new JsonMessageDeserializer<TMessage>())
                .SetMessageReadStrategy(new MessageReadQueueStrategy())
                .Subscribe(handler)
                .CreateDefaultBinding()
                .SetLogger(_log)
                .Start();
        }
    }
}
