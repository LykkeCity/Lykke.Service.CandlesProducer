using System;

namespace Lykke.Job.CandlesProducer.Core.Domain.Trades
{
    public class Trade
    {
        public string AssetPair { get; }
        public DateTime Timestamp { get; }
        /// <summary>
        /// Volume in the asset pair base asset
        /// </summary>
        public double BaseVolume { get; }
        /// <summary>
        /// Volume in the asset pair quoting asset
        /// </summary>
        public double QuotingVolume { get; }
        public double Price { get; }

        public Trade(string assetPair, DateTime timestamp, double baseVolume, double quotingVolume, double price)
        {
            AssetPair = assetPair;
            Timestamp = timestamp;
            BaseVolume = Math.Abs(baseVolume);
            QuotingVolume = Math.Abs(quotingVolume);
            Price = price;
        }
    }
}
