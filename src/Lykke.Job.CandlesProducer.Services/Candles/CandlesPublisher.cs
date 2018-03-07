using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Job.CandlesProducer.Contract;
using Lykke.Job.CandlesProducer.Core.Domain.Candles;
using Lykke.Job.CandlesProducer.Core.Services;
using Lykke.Job.CandlesProducer.Core.Services.Candles;
using Lykke.RabbitMqBroker.Publisher;

namespace Lykke.Job.CandlesProducer.Services.Candles
{
    [UsedImplicitly]
    public class CandlesPublisher : ICandlesPublisher
    {
        private readonly IRabbitMqPublishersFactory _publishersFactory;
        private readonly IRabbitPublicationSettings _settings;

        private RabbitMqPublisher<CandlesUpdatedEvent> _publisher;

        public CandlesPublisher(IRabbitMqPublishersFactory publishersFactory, IRabbitPublicationSettings settings)
        {
            _publishersFactory = publishersFactory;
            _settings = settings;
        }

        public void Start()
        {
            _publisher = _publishersFactory.Create(
                new MessagePackMessageSerializer<CandlesUpdatedEvent>(),
                _settings.ConnectionString,
                _settings.Namespace,
                "candles-v2");
        }

        public Task PublishAsync(IReadOnlyCollection<CandleUpdateResult> updates)
        {
            return PublishV2Async(updates);
        }

        private Task PublishV2Async(IEnumerable<CandleUpdateResult> updates)
        {
            var @event = new CandlesUpdatedEvent
            {
                ContractVersion = Contract.Constants.ContractVersion,
                UpdateTimestamp = DateTime.UtcNow,
                Candles = updates
                    .Select(c => new CandleUpdate
                    {
                        IsLatestChange = c.IsLatestChange,
                        ChangeTimestamp = c.Candle.LatestChangeTimestamp,
                        AssetPairId = c.Candle.AssetPairId,
                        PriceType = c.Candle.PriceType,
                        TimeInterval = c.Candle.TimeInterval,
                        CandleTimestamp = c.Candle.Timestamp,
                        Open = c.Candle.Open,
                        Close = c.Candle.Close,
                        Low = c.Candle.Low,
                        High = c.Candle.High,
                        TradingVolume = c.Candle.TradingVolume,
                        TradingOppositeVolume = c.Candle.TradingOppositeVolume,
                        LastTradePrice = c.Candle.LastTradePrice
                    })
                    .ToArray()
            };

            lock (_publisher)
            {
                // HACK: Actually ProduceAsync is not async, so lock works well

                return _publisher.ProduceAsync(@event);
            }
        }

        public void Dispose()
        {
            _publisher?.Dispose();
        }

        public void Stop()
        {
            _publisher?.Stop();
        }
    }
}
