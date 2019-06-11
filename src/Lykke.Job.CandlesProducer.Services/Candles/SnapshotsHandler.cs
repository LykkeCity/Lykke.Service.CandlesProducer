using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Common;
using Lykke.Common.Log;
using Lykke.Job.CandlesProducer.Core.Services;

namespace Lykke.Job.CandlesProducer.Services.Candles
{
    public class SnapshotsHandler : IStartable, IDisposable
    {
        private readonly IEnumerable<ISnapshotSerializer> _snapshotSerializers;
        private readonly TimerTrigger _timerTrigger;

        public SnapshotsHandler(
            ILogFactory logFactory,
            TimeSpan period,
            IEnumerable<ISnapshotSerializer> snapshotSerializers
            )
        {
            _snapshotSerializers = snapshotSerializers;
            _timerTrigger = new TimerTrigger(nameof(SnapshotsHandler), period, logFactory);
            _timerTrigger.Triggered += Execute;

        }

        public void Start()
        {
            _timerTrigger.Start();
        }

        public void Dispose()
        {
            _timerTrigger?.Stop();
            _timerTrigger?.Dispose();
        }

        private Task Execute(ITimerTrigger timer, TimerTriggeredHandlerArgs args, CancellationToken cancellationToken)
        {
            var snapshotSrializationTasks = _snapshotSerializers.Select(s  => s.SerializeAsync());

            return Task.WhenAll(snapshotSrializationTasks);
        }
    }
}
