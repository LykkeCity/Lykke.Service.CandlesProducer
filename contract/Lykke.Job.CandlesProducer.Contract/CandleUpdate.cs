using System;
using JetBrains.Annotations;
using MessagePack;

namespace Lykke.Job.CandlesProducer.Contract
{
    /// <summary>
    /// Candle update
    /// </summary>
    [PublicAPI]
    [MessagePackObject]
    public class CandleUpdate
    {
        /// <summary>
        /// Is the latest change of the given candle at the moment of the update?
        /// </summary>
        /// <remarks>
        /// For example, candle can be changed by the trade with timestamp 00:00:01.100 and then
        /// by the quote with the early timestamp 00:00:01.050. Thus <see cref="IsLatestChange"/> for the
        /// first (made by the trade) change will be true, and <see cref="IsLatestChange"/> for the
        /// second (made by the quote) change will be false.
        /// </remarks>
        [Key(1)]
        public bool IsLatestChange { get; set; }

        /// <summary>
        /// Timestamp of the quote or trade, one of which generated this update
        /// </summary>
        [Key(2)]
        public DateTime ChangeTimestamp { get; set; }

        /// <summary>
        /// Asset pair ID of the candle
        /// </summary>
        [Key(3)]
        public string AssetPairId { get; set; }

        /// <summary>
        /// Price type of the candle
        /// </summary>
        [Key(4)]
        public CandlePriceType PriceType { get; set; }

        /// <summary>
        /// Time interval of the candle
        /// </summary>
        [Key(5)]
        public CandleTimeInterval TimeInterval { get; set; }

        /// <summary>
        /// Timestamp when the candle was opened
        /// </summary>
        /// <remarks>
        /// It always truncated to the seconds for the <see cref="CandleTimeInterval.Sec"/>, 
        /// to the minutes for the <see cref="CandleTimeInterval.Minute"/>, etc.
        /// </remarks>
        [Key(6)]
        public DateTime CandleTimestamp { get; set; }

        /// <summary>
        /// Open price of the candle
        /// </summary>
        /// <remarks>
        /// Can be changed even if you already process given <see cref="CandleTimestamp"/> 
        /// for the given <see cref="AssetPairId"/>, <see cref="PriceType"/> and <see cref="TimeInterval"/>
        /// </remarks>
        [Key(7)]
        public double Open { get; set; }

        /// <summary>
        /// Close price of the candle
        /// </summary>
        [Key(8)]
        public double Close { get; set; }

        /// <summary>
        /// Highest price of the candle
        /// </summary>
        [Key(9)]
        public double High { get; set; }

        /// <summary>
        /// Lowest price of the candle
        /// </summary>
        [Key(10)]
        public double Low { get; set; }

        /// <summary>
        /// Trading volume of the candle in the base asset of the asset pair.
        /// Has the meaning only for the <see cref="PriceType"/> == <see cref="CandlePriceType.Trades"/>
        /// </summary>
        [Key(11)]
        public double TradingVolume { get; set; }

        /// <summary>
        /// Trading volume of the candle in the quoting asset of the asset pair
        /// Has the meaning only for the <see cref="PriceType"/> == <see cref="CandlePriceType.Trades"/>
        /// </summary>
        [Key(13)]
        public double TradingOppositeVolume { get; set; }
    }
}
