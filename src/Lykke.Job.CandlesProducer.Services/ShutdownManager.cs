using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Job.CandlesProducer.Core.Services;
using Lykke.Job.CandlesProducer.Core.Services.Candles;
using Lykke.Job.CandlesProducer.Core.Services.Quotes;
using Lykke.Job.CandlesProducer.Core.Services.Trades;
using Lykke.Sdk;

namespace Lykke.Job.CandlesProducer.Services
{
    // TODO: Stop MT trades subscriber

    public class ShutdownManager : IShutdownManager
    {
        private readonly IQuotesSubscriber _quotesSubscriber;
        private readonly ITradesSubscriber _tradesSubscriber;
        private readonly ICandlesPublisher _publisher;
        private readonly IEnumerable<ISnapshotSerializer> _snapshotSerializers;
        private readonly ILog _log;

        public ShutdownManager(
            IQuotesSubscriber quotesSubscriber,
            ITradesSubscriber tradesSubscriber,
            ICandlesPublisher publisher,
            IEnumerable<ISnapshotSerializer> snapshotSerializerses,
            ILogFactory logFactory)
        {
            _quotesSubscriber = quotesSubscriber;
            _tradesSubscriber = tradesSubscriber;
            _publisher = publisher;
            _snapshotSerializers = snapshotSerializerses;
            _log = logFactory.CreateLog(this);
        }

        public async Task StopAsync()
        {
            _log.Info(nameof(StopAsync), "Stopping trades subscriber...");

            _tradesSubscriber.Stop();

            _log.Info(nameof(StopAsync), "Stopping quotes subscriber...");

            _quotesSubscriber.Stop();

            _log.Info(nameof(StopAsync), "Serializing snapshots async...");
            
            var snapshotSrializationTasks = _snapshotSerializers.Select(s  => s.SerializeAsync());

            _log.Info(nameof(StopAsync), "Stopping candles publisher...");

            _publisher.Stop();

            _log.Info(nameof(StopAsync), "Awaiting for snapshots serialization...");

            await Task.WhenAll(snapshotSrializationTasks);

            _log.Info(nameof(StopAsync), "Shutted down");
        }
    }
}
