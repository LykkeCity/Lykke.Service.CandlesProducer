using JetBrains.Annotations;

namespace Lykke.Job.CandlesProducer.Contract
{
    [PublicAPI]
    public enum CandlePriceType
    {
        Unspecified = 0,
        Bid = 1,
        Ask = 2,
        Mid = 3,
        Trades = 4
    }
}
