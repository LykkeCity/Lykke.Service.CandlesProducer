using System;

namespace Lykke.Job.CandlesProducer.Settings
{
    public class CandlesGeneratorSettings
    {
        public TimeSpan OldDataWarningTimeout { get; set; }
        public TimeSpan PersistSnapshotInterval { get; set; }
    }
}
