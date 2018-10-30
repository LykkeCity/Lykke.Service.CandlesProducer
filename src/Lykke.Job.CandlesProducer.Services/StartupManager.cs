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
    // TODO: Start MT trades subscriber

    public class StartupManager : IStartupManager
    {
        private readonly IQuotesSubscriber _quotesSubscriber;
        private readonly ITradesSubscriber _tradesSubscriber;
        private readonly ICandlesPublisher _candlesPublisher;
        private readonly IEnumerable<ISnapshotSerializer> _snapshotSerializers;
        private readonly ILog _log;

        public StartupManager(
            IQuotesSubscriber quotesSubscriber,
            ITradesSubscriber tradesSubscriber,
            ICandlesPublisher candlesPublisher,
            IEnumerable<ISnapshotSerializer> snapshotSerializers,
            ILogFactory logFactory)
        {
            _quotesSubscriber = quotesSubscriber;
            _tradesSubscriber = tradesSubscriber;
            _candlesPublisher = candlesPublisher;
            _snapshotSerializers = snapshotSerializers;
            _log = logFactory.CreateLog(this);
        }

        public  async Task StartAsync()
        {
            _log.Info(nameof(StartAsync), "Deserializing snapshots async...");

            var snapshotTasks = _snapshotSerializers.Select(s => s.DeserializeAsync()).ToArray();

            _log.Info(nameof(StartAsync), "Starting candles publisher...");

            _candlesPublisher.Start();

            _log.Info(nameof(StartAsync), "Waiting for snapshots async...");

            await Task.WhenAll(snapshotTasks);

            _log.Info(nameof(StartAsync), "Starting quotes subscriber...");

            _quotesSubscriber.Start();

            _log.Info(nameof(StartAsync), "Starting trades subscriber...");

            _tradesSubscriber.Start();

            _log.Info(nameof(StartAsync), "Started up");
        }
    }
}
