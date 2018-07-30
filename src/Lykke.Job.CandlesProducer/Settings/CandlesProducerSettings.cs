using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.CandlesProducer.Settings
{
    public class CandlesProducerSettings
    {
        [Optional , CanBeNull]
        public ResourceMonitorSettings ResourceMonitor { get; set; }
        public DbSettings Db { get; set; }
        public AssetsCacheSettings AssetsCache { get; set; }
        public RabbitSettings Rabbit { get; set; }
        public CandlesGeneratorSettings CandlesGenerator { get; set; }
    }
}
