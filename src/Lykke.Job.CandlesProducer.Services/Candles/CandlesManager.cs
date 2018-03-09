using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Job.CandlesProducer.Core.Domain.Trades;
using Lykke.Job.CandlesProducer.Core.Services.Assets;
using Lykke.Job.CandlesProducer.Core.Services.Candles;
using JetBrains.Annotations;
using Lykke.Job.CandlesProducer.Contract;
using Lykke.Job.CandlesProducer.Core.Domain.Candles;
using Lykke.Job.QuotesProducer.Contract;

namespace Lykke.Job.CandlesProducer.Services.Candles
{
    [UsedImplicitly]
    public class CandlesManager : ICandlesManager
    {
        private readonly IMidPriceQuoteGenerator _midPriceQuoteGenerator;
        private readonly IAssetPairsManager _assetPairsManager;
        private readonly ICandlesGenerator _candlesGenerator;
        private readonly ICandlesPublisher _publisher;

        public CandlesManager(
            IMidPriceQuoteGenerator midPriceQuoteGenerator,
            IAssetPairsManager assetPairsManager,
            ICandlesGenerator candlesGenerator,
            ICandlesPublisher publisher)
        {
            _midPriceQuoteGenerator = midPriceQuoteGenerator;
            _assetPairsManager = assetPairsManager;
            _candlesGenerator = candlesGenerator;
            _publisher = publisher;
        }

        public async Task ProcessQuoteAsync(QuoteMessage quote)
        {
            var assetPair = await _assetPairsManager.TryGetEnabledPairAsync(quote.AssetPair);

            if (assetPair == null)
            {
                return;
            }

            var changedUpdates = new ConcurrentBag<CandleUpdateResult>();
            var midPriceQuote = _midPriceQuoteGenerator.TryGenerate(
                quote.AssetPair,
                quote.IsBuy,
                quote.Price,
                quote.Timestamp,
                assetPair.Accuracy);

            try
            {
                // Updates all intervals in parallel

                var processingTasks = Constants
                    .PublishedIntervals
                    .Select(timeInterval => Task.Factory.StartNew(() =>
                    {
                        ProcessQuoteInterval(
                            quote.AssetPair,
                            quote.Timestamp,
                            quote.Price,
                            quote.IsBuy,
                            timeInterval,
                            midPriceQuote,
                            changedUpdates);
                    }));

                await Task.WhenAll(processingTasks);

                // Publishes updated candles

                if (!changedUpdates.IsEmpty)
                {
                    await _publisher.PublishAsync(changedUpdates);
                }
            }
            catch (Exception)
            {
                // Failed to publish one or several candles, so processing should be cancelled

                foreach (var updateResult in changedUpdates)
                {
                    _candlesGenerator.Undo(updateResult);
                }

                throw;
            }
        }

        public async Task ProcessTradeAsync(Trade trade)
        {
            var changedUpdates = new ConcurrentBag<CandleUpdateResult>();

            try
            {
                // Updates all intervals in parallel

                var processingTasks = Constants
                    .PublishedIntervals
                    .Select(timeInterval => Task.Factory.StartNew(() =>
                    {
                        ProcessTradeInterval(
                            trade.AssetPair,
                            trade.Timestamp,
                            trade.Price,
                            trade.BaseVolume,
                            trade.QuotingVolume,
                            timeInterval, 
                            changedUpdates);
                    }));

                await Task.WhenAll(processingTasks);

                // Publishes updated candles

                if (!changedUpdates.IsEmpty)
                {
                    await _publisher.PublishAsync(changedUpdates);
                }
            }
            catch (Exception)
            {
                // Failed to publish one or several candles, so processing should be cancelled

                foreach (var updateResult in changedUpdates)
                {
                    _candlesGenerator.Undo(updateResult);
                }

                throw;
            }
        }

        private void ProcessQuoteInterval(
            string assetPair,
            DateTime timestamp,
            double price,
            bool isBuy,
            CandleTimeInterval timeInterval,
            QuoteMessage midPriceQuote,
            ConcurrentBag<CandleUpdateResult> changedUpdateResults)
        {
            // Updates ask/bid candle

            var candleUpdateResult = _candlesGenerator.UpdateQuotingCandle(
                assetPair,
                timestamp,
                price,
                isBuy ? CandlePriceType.Bid : CandlePriceType.Ask,
                timeInterval);

            if (candleUpdateResult.WasChanged)
            {
                changedUpdateResults.Add(candleUpdateResult);
            }

            // Updates mid candle

            if (midPriceQuote != null)
            {
                var midPriceCandleUpdateResult = _candlesGenerator.UpdateQuotingCandle(
                    midPriceQuote.AssetPair,
                    midPriceQuote.Timestamp,
                    midPriceQuote.Price,
                    CandlePriceType.Mid,
                    timeInterval);

                if (midPriceCandleUpdateResult.WasChanged)
                {
                    changedUpdateResults.Add(midPriceCandleUpdateResult);
                }
            }
        }

        private void ProcessTradeInterval(string assetPair, DateTime timestamp, double tradePrice, double baseVolume, double quotingVolume, CandleTimeInterval timeInterval, ConcurrentBag<CandleUpdateResult> changedUpdateResults)
        {
            var candleUpdateResult = _candlesGenerator.UpdateTradingCandle(
                assetPair,
                timestamp,
                tradePrice,
                baseVolume, 
                quotingVolume, 
                timeInterval);

            if (candleUpdateResult.WasChanged)
            {
                changedUpdateResults.Add(candleUpdateResult);
            }
        }
    }
}
