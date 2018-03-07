namespace Lykke.Job.CandlesProducer.Core.Domain.Candles
{
    public class CandleUpdateResult
    {
        public static readonly CandleUpdateResult Empty = new CandleUpdateResult(null, null, false, false);

        public Candle Candle { get; }
        public Candle OldCandle { get; }
        public bool WasChanged { get; }
        public bool IsLatestChange { get; }

        public CandleUpdateResult(Candle candle, Candle oldCandle, bool wasChanged, bool isLatestChange)
        {
            Candle = candle;
            OldCandle = oldCandle;
            WasChanged = wasChanged;
            IsLatestChange = isLatestChange;
        }
    }
}
