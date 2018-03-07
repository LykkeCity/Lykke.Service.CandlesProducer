using System;
using System.Collections.Immutable;
using Lykke.Job.CandlesProducer.Contract;
using Lykke.Job.CandlesProducer.Core.Domain.Candles;

namespace Lykke.Job.CandlesProducer.Core.Services.Candles
{
    public interface ICandlesGenerator : IHaveState<ImmutableDictionary<string, ICandle>>
    {
        CandleUpdateResult UpdatePrice(string assetPair, DateTime timestamp, double price, CandlePriceType priceType, CandleTimeInterval timeInterval);
        CandleUpdateResult UpdateTradingVolume(string assetPair, DateTime timestamp, double tradingVolume, double tradingOppositeVolume, double tradePrice, CandleTimeInterval timeInterval);
        void Undo(CandleUpdateResult candleUpdateResult);
    }
}
