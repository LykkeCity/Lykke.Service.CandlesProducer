using System;
using JetBrains.Annotations;
using Lykke.Job.CandlesProducer.Contract;
using Lykke.Job.CandlesProducer.Core.Domain.Candles;
using MessagePack;

namespace Lykke.Job.CandlesProducer.AzureRepositories
{
    [MessagePackObject]
    public class CandleEntity : ICandle
    {
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        [Key(0)]
        public string AssetPairId { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        [Key(1)]
        public CandlePriceType PriceType { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        [Key(2)]
        public CandleTimeInterval TimeInterval { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        [Key(3)]
        public DateTime Timestamp { get; set; }

        [UsedImplicitly]
        [Key(4)]
        public decimal Open { get; set; }

        [UsedImplicitly]
        [Key(5)]
        public decimal Close { get; set; }

        [UsedImplicitly]
        [Key(6)]
        public decimal High { get; set; }

        [UsedImplicitly]
        [Key(7)]
        public decimal Low { get; set; }

        [UsedImplicitly]
        [Key(8)]
        public decimal TradingVolume { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        [Key(9)]
        public DateTime LatestChangeTimestamp { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        [Key(10)]
        public DateTime OpenTimestamp { get; set; }

        [UsedImplicitly]
        [Key(13)]
        public decimal TradingOppositeVolume { get; set; }

        double ICandle.Open => (double) Open;

        double ICandle.Close => (double) Close;

        double ICandle.High => (double) High;

        double ICandle.Low => (double) Low;

        double ICandle.TradingVolume => (double) TradingVolume;

        double ICandle.TradingOppositeVolume => (double) TradingOppositeVolume;

        public static CandleEntity Copy(ICandle candle)
        {
            return new CandleEntity
            {
                AssetPairId = candle.AssetPairId,
                PriceType = candle.PriceType,
                TimeInterval = candle.TimeInterval,
                Timestamp = candle.Timestamp,
                Open = (decimal) candle.Open,
                Close = (decimal) candle.Close,
                Low = (decimal) candle.Low,
                High = (decimal) candle.High,
                TradingVolume = (decimal) candle.TradingVolume,
                TradingOppositeVolume = (decimal) candle.TradingOppositeVolume,
                LatestChangeTimestamp = candle.LatestChangeTimestamp,
                OpenTimestamp = candle.OpenTimestamp
            };
        }
    }
}
