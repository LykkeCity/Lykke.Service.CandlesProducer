using System;
using Lykke.Job.CandlesProducer.Contract;

namespace Lykke.Job.CandlesProducer.Core.Domain.Candles
{
    public class Candle : IEquatable<Candle>, ICandle
    {
        public string AssetPairId { get; }
        public CandlePriceType PriceType { get; }
        public CandleTimeInterval TimeInterval { get; }
        public DateTime Timestamp { get; }
        public double Open { get; }
        public double Close { get; }
        public double High { get; }
        public double Low { get; }
        public double TradingVolume { get; }
        public double TradingOppositeVolume { get; }
        public double LastTradePrice { get; }
        public DateTime LatestChangeTimestamp { get; }
        public DateTime OpenTimestamp { get; }
        public bool HasPrices { get; }


        private Candle(
            string assetPairId, 
            CandlePriceType priceType, 
            CandleTimeInterval timeInterval, 
            DateTime timestamp, 
            DateTime latestChangeTimestamp,
            DateTime openTimestamp,
            double open, 
            double close, 
            double low, 
            double high,
            double tradingVolume,
            double tradingOppositeVolume,
            double lastTradePrice,
            bool hasPrices)
        {
            AssetPairId = assetPairId;
            PriceType = priceType;
            TimeInterval = timeInterval;
            Timestamp = timestamp;
            LatestChangeTimestamp = latestChangeTimestamp;
            OpenTimestamp = openTimestamp;
            Open = open;
            Close = close;
            Low = low;
            High = high;
            // Here and below we assume that candles of types ask/bid/mid can not have trading volumes and\or last trade price.
            // I don't think it would be gracefull to throw any exception on this issue, but silent zeroing of the properties
            // seems to be pretty nice behavior.
            TradingVolume = priceType != CandlePriceType.Trades ? 0 : tradingVolume;
            TradingOppositeVolume = priceType != CandlePriceType.Trades ? 0 : tradingOppositeVolume;
            LastTradePrice = priceType != CandlePriceType.Trades ? 0 : lastTradePrice;
            HasPrices = hasPrices;
        }

        public static Candle Copy(ICandle candle)
        {
            return new Candle
            (
                candle.AssetPairId,
                candle.PriceType,
                candle.TimeInterval,
                candle.Timestamp,
                candle.LatestChangeTimestamp,
                candle.OpenTimestamp,
                candle.Open,
                candle.Close,
                candle.Low,
                candle.High,
                candle.TradingVolume,
                candle.TradingOppositeVolume, 
                candle.LastTradePrice,
                candle.HasPrices
            );
        }

        public static Candle CreateWithPrice(
            string assetPair,
            DateTime timestamp,
            double price, 
            double lastTradePrice,
            CandlePriceType priceType, 
            CandleTimeInterval timeInterval)
        {
            var intervalTimestamp = timestamp.TruncateTo(timeInterval);

            return new Candle
            (
                assetPair,
                priceType,
                timeInterval,
                intervalTimestamp,
                timestamp,
                timestamp,
                price,
                price,
                price,
                price,
                0,
                0,
                priceType != CandlePriceType.Trades ? 0 : lastTradePrice,
                true
            );
        }

        public static Candle CreateWithTradingVolume(
            string assetPair,
            DateTime timestamp, 
            double tradingVolume, 
            double tradingOppositeVolume, 
            double lastTradePrice, 
            CandlePriceType priceType,
            CandleTimeInterval timeInterval)
        {
            var intervalTimestamp = timestamp.TruncateTo(timeInterval);

            return new Candle
            (
                assetPair,
                priceType,
                timeInterval,
                intervalTimestamp,
                timestamp,
                timestamp,
                0,
                0,
                0,
                0,
                priceType != CandlePriceType.Trades ? 0 : tradingVolume,
                priceType != CandlePriceType.Trades ? 0 : tradingOppositeVolume,
                priceType != CandlePriceType.Trades ? 0 : lastTradePrice,
                false
            );
        }

        public Candle UpdatePrice(DateTime timestamp, double price)
        {
            double closePrice;
            double openPrice;
            double lowPrice;
            double highPrice;

            if (HasPrices)
            {
                closePrice = LatestChangeTimestamp < timestamp ? price : Close;
                openPrice = OpenTimestamp > timestamp ? price : Open;
                lowPrice = Math.Min(Low, price);
                highPrice = Math.Max(High, price);
            }
            else
            {
                openPrice = price;
                closePrice = price;
                lowPrice = price;
                highPrice = price;
            }

            var changeTimestamp = LatestChangeTimestamp < timestamp ? timestamp : LatestChangeTimestamp;
            var openTimestamp = OpenTimestamp > timestamp ? timestamp : OpenTimestamp;

            return new Candle(
                AssetPairId,
                PriceType,
                TimeInterval,
                Timestamp,
                changeTimestamp,
                openTimestamp,
                openPrice,
                closePrice,
                lowPrice,
                highPrice,
                PriceType != CandlePriceType.Trades ? 0 : TradingVolume,
                PriceType != CandlePriceType.Trades ? 0 : TradingOppositeVolume,
                PriceType != CandlePriceType.Trades ? 0 : LastTradePrice,
                true);
        }

        public Candle UpdateTradingVolume(DateTime timestamp, double tradingVolume, double tradingOppositeVolume, double lastTradePrice)
        {
            var changeTimestamp = LatestChangeTimestamp < timestamp ? timestamp : LatestChangeTimestamp;
            var localLastTradePrice = LatestChangeTimestamp < timestamp ? lastTradePrice : LastTradePrice;

            return new Candle(
                AssetPairId,
                PriceType,
                TimeInterval,
                Timestamp,
                changeTimestamp,
                OpenTimestamp,
                Open,
                Close,
                Low,
                High,
                PriceType != CandlePriceType.Trades ? 0 : TradingVolume + tradingVolume,
                PriceType != CandlePriceType.Trades ? 0 : TradingOppositeVolume + tradingOppositeVolume,
                PriceType != CandlePriceType.Trades ? 0 : localLastTradePrice,
                HasPrices);
        }

        public Candle SubstractVolume(double tradingVolume, double tradingOppositeVolume)
        {
            return new Candle(
                AssetPairId,
                PriceType,
                TimeInterval,
                Timestamp,
                LatestChangeTimestamp,
                OpenTimestamp,
                Open,
                Close,
                Low,
                High,
                PriceType != CandlePriceType.Trades ? 0 : TradingVolume - tradingVolume,
                PriceType != CandlePriceType.Trades ? 0 : TradingOppositeVolume - tradingOppositeVolume,
                PriceType != CandlePriceType.Trades ? 0 : LastTradePrice,
                HasPrices);
        }

        public bool Equals(Candle other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(AssetPairId, other.AssetPairId) &&
                   PriceType == other.PriceType &&
                   TimeInterval == other.TimeInterval &&
                   Timestamp.Equals(other.Timestamp) &&
                   LatestChangeTimestamp.Equals(other.LatestChangeTimestamp) &&
                   Open.Equals(other.Open) &&
                   Close.Equals(other.Close) &&
                   High.Equals(other.High) &&
                   Low.Equals(other.Low) &&
                   TradingVolume.Equals(other.TradingVolume) &&
                   TradingOppositeVolume.Equals(other.TradingOppositeVolume) &&
                   LastTradePrice.Equals(other.LastTradePrice) &&
                   HasPrices.Equals(other.HasPrices);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((Candle)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = AssetPairId != null ? AssetPairId.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (int)PriceType;
                hashCode = (hashCode * 397) ^ (int)TimeInterval;
                hashCode = (hashCode * 397) ^ Timestamp.GetHashCode();
                hashCode = (hashCode * 397) ^ LatestChangeTimestamp.GetHashCode();
                hashCode = (hashCode * 397) ^ Open.GetHashCode();
                hashCode = (hashCode * 397) ^ Close.GetHashCode();
                hashCode = (hashCode * 397) ^ High.GetHashCode();
                hashCode = (hashCode * 397) ^ Low.GetHashCode();
                hashCode = (hashCode * 397) ^ TradingVolume.GetHashCode();
                hashCode = (hashCode * 397) ^ TradingOppositeVolume.GetHashCode();
                hashCode = (hashCode * 397) ^ LastTradePrice.GetHashCode();
                hashCode = (hashCode * 397) ^ HasPrices.GetHashCode();

                return hashCode;
            }
        }
    }
}
