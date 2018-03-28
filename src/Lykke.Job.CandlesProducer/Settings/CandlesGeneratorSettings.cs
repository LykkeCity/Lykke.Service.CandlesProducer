using System;

namespace Lykke.Job.CandlesProducer.Settings
{
    public class CandlesGeneratorSettings
    {
        public TimeSpan MinCacheAge { get; set; }
        public TimeSpan OldDataWarningTimeout { get; set; }
    }
}
