using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Job.CandlesProducer.Contract;
using Lykke.Job.CandlesProducer.Core.Domain.Candles;
using Lykke.Job.CandlesProducer.Core.Services.Candles;

namespace Lykke.Job.CandlesProducer.Services.Candles
{
    [UsedImplicitly]
    public class CandlesGenerator : ICandlesGenerator
    {
        private readonly ILog _log;
        private readonly TimeSpan _minCacheAge;
        private ConcurrentDictionary<string, Candle> _candles;
        
        public CandlesGenerator(ILog log, TimeSpan minCacheAge)
        {
            _log = log;
            _minCacheAge = minCacheAge;
            _candles = new ConcurrentDictionary<string, Candle>();
        }

        public CandleUpdateResult UpdatePrice(string assetPair, DateTime timestamp, double price, CandlePriceType priceType, CandleTimeInterval timeInterval)
        {
            // We can update LastTradePrice for only Trades candle below:
            return Update(assetPair, timestamp, priceType, timeInterval,
                createNewCandle: oldCandle => Candle.CreateWithPrice(assetPair, timestamp, price, priceType == CandlePriceType.Trades ? oldCandle?.LastTradePrice ?? 0 : 0, priceType, timeInterval),
                updateCandle: oldCandle => oldCandle.UpdatePrice(timestamp, price),
                getLoggingContext: candle => new
                {
                    assetPair = assetPair,
                    timestamp = timestamp
                });
        }

        public CandleUpdateResult UpdateTradingVolume(string assetPair, DateTime timestamp, double tradingVolume, double tradingOppositeVolume, double tradePrice, CandleTimeInterval timeInterval)
        {
            return Update(assetPair, timestamp, CandlePriceType.Trades, timeInterval,
                createNewCandle: oldCandle => Candle.CreateWithTradingVolume(assetPair, timestamp, tradingVolume, tradingOppositeVolume, tradePrice, CandlePriceType.Trades, timeInterval), 
                updateCandle: oldCandle => oldCandle.UpdateTradingVolume(timestamp, tradingVolume, tradingOppositeVolume, tradePrice),
                getLoggingContext: candle => new
                {
                    assetPair = assetPair,
                    timestamp = timestamp,
                    baseVolume = tradingVolume,
                    quotingVolume = tradingOppositeVolume,
                    tradePrice = tradePrice
                });
        }
        
        public void Undo(CandleUpdateResult candleUpdateResult)
        {
            if (!candleUpdateResult.WasChanged)
            {
                return;
            }

            var candle = candleUpdateResult.Candle;
            var key = GetKey(candle.AssetPairId, candle.TimeInterval, candle.PriceType);

            // Key should be presented in the dictionary, since Update was called before and keys are never removed

            _candles.AddOrUpdate(
                key,
                addValueFactory: k => throw new InvalidOperationException("Key should be already presented in the dictionary"),
                updateValueFactory: (k, candles) => Candle.Copy(candleUpdateResult.OldCandle)
                );
        }

        public ImmutableDictionary<string, ICandle> GetState()
        {
            return _candles.ToImmutableDictionary(i => i.Key, i => (ICandle)i.Value);
        }

        public void SetState(ImmutableDictionary<string, ICandle> state)
        {
            if (_candles.Count > 0)
            {
                throw new InvalidOperationException("Candles generator state already not empty");
            }

            var keyValuePairs = state.Select(i => KeyValuePair.Create(i.Key, Candle.Copy(i.Value)));

            _candles = new ConcurrentDictionary<string, Candle>(keyValuePairs);
        }

        public string DescribeState(ImmutableDictionary<string, ICandle> state)
        {
            return $"Candles count: {state.Count}";
        }

        private CandleUpdateResult Update(
            string assetPair,
            DateTime timestamp,
            CandlePriceType priceType,
            CandleTimeInterval timeInterval,
            Func<Candle, Candle> createNewCandle,
            Func<Candle, Candle> updateCandle,
            Func<Candle, object> getLoggingContext)
        {
            var key = GetKey(assetPair, timeInterval, priceType);

            // Let's prepare the old and the updated versions of our candle to compare in the very end.
            _candles.TryGetValue(key, out var candleBefore);
            Candle candleAfter = null;
            
            _candles.AddOrUpdate(key,
                addValueFactory: k => createNewCandle(null),
                updateValueFactory: (k, candle) =>
                {
                    lock (candle)
                    {
                        // Candles is identified by the Timestamp

                        var candleTimestamp = timestamp.TruncateTo(timeInterval);
                        
                        if (candleTimestamp == candle.Timestamp)    // Candle matches exactly - updating it
                            candleAfter = updateCandle(candle);

                        else if (candleTimestamp > candle.Timestamp)    // The timestamp is newer than the candle we have - creating new candle instead
                            candleAfter = createNewCandle(candle);

                        else    // Given data is older then the oldest of the cached candles.
                        {
                            // Nothing to update here and no candle can be returned
                            // since we can't obtain full candle state

                            _log.WriteWarningAsync(
                                nameof(CandlesGenerator),
                                nameof(Update),
                                getLoggingContext(candle).ToJson(),
                                "Incoming data is too old to update the candle. No candle will be altered.").Wait();
                        }

                        return (candleAfter != null && candleAfter.HasPrices) ? candleAfter : Candle.Copy(candle);   // If we have a problem, we do not update the candle.
                    }
                });

            // If we just added a candle for this key at first time, we need to set candleAfter manually. Otherwise, we will get an ampty update result.
            if (candleBefore == null)
                _candles.TryGetValue(key, out candleAfter);

            // Candles without prices shouldn't be produced
            return candleAfter == null || !candleAfter.HasPrices
                ? CandleUpdateResult.Empty
                : new CandleUpdateResult(
                    candleAfter,
                    candleBefore,
                    wasChanged: !candleAfter.Equals(candleBefore),
                    isLatestChange: candleBefore == null || candleAfter.LatestChangeTimestamp >= candleBefore.LatestChangeTimestamp);
        }

        private static string GetKey(string assetPairId, CandleTimeInterval timeInterval, CandlePriceType priceType)
        {
            return $"{assetPairId.Trim().ToUpper()}-{priceType}-{timeInterval}";
        }
    }
}
