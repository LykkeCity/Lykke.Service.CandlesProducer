using Lykke.Job.CandlesProducer.Core.Services.Quotes;

namespace Lykke.Job.CandlesProducer.Services.Quotes
{
    public class EmptyQuotesSubscriber : IQuotesSubscriber
    {
        public void Start()
        {
            // Just do nothing.
        }

        public void Dispose()
        {
            // Just do nothing.
        }

        public void Stop()
        {
            // Just do nothing.
        }
    }
}
