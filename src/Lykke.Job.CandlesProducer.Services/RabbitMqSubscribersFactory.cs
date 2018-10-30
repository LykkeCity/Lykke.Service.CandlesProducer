using System;
using System.Threading.Tasks;
using Common;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Job.CandlesProducer.Core.Services;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;

namespace Lykke.Job.CandlesProducer.Services
{
    [UsedImplicitly]
    public class RabbitMqSubscribersFactory : IRabbitMqSubscribersFactory
    {
        private readonly ILogFactory _logFactory;

        public RabbitMqSubscribersFactory(ILogFactory logFactory)
        {
            _logFactory = logFactory;
        }

        public IStopable Create<TMessage>(string connectionString, string @namespace, string source, Func<TMessage, Task> handler, string queueSuffix = null)
        {
            var settings = RabbitMqSubscriptionSettings
                .ForSubscriber(connectionString, @namespace, source, @namespace, $"candlesproducer{queueSuffix}")
                .MakeDurable();

            return new RabbitMqSubscriber<TMessage>(_logFactory, settings,
                    new ResilientErrorHandlingStrategy(_logFactory, settings,
                        retryTimeout: TimeSpan.FromSeconds(10),
                        retryNum: 10,
                        next: new DeadQueueErrorHandlingStrategy(_logFactory, settings)))
                .SetMessageDeserializer(new JsonMessageDeserializer<TMessage>())
                .SetMessageReadStrategy(new MessageReadQueueStrategy())
                .Subscribe(handler)
                .CreateDefaultBinding()
                .Start();
        }
    }
}
