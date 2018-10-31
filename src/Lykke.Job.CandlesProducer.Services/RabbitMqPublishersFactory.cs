using System;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Job.CandlesProducer.Core.Services;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.RabbitMqBroker.Subscriber;

namespace Lykke.Job.CandlesProducer.Services
{
    [UsedImplicitly]
    public class RabbitMqPublishersFactory : IRabbitMqPublishersFactory
    {
        private readonly ILogFactory _logFactory;
        private readonly ILog _log;

        public RabbitMqPublishersFactory(ILogFactory logFactory)
        {
            _logFactory = logFactory;
            _log = logFactory.CreateLog(this);
        }

        public RabbitMqPublisher<TMessage> Create<TMessage>(
            IRabbitMqSerializer<TMessage> serializer, 
            string connectionString, 
            string @namespace, 
            string endpoint)
        {
            try
            {
                var settings = RabbitMqSubscriptionSettings
                    .ForPublisher(connectionString, @namespace, endpoint)
                    .MakeDurable();

                return new RabbitMqPublisher<TMessage>(_logFactory, settings)
                    .SetSerializer(serializer)
                    .SetPublishStrategy(new DefaultFanoutPublishStrategy(settings))
                    .PublishSynchronously()
                    .Start();
            }
            catch (Exception ex)
            {
                _log.Error(nameof(Create), ex);
                throw;
            }
        }
    }
}
