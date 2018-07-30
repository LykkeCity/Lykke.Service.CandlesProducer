using Lykke.Job.CandlesProducer.Core.Domain;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.CandlesProducer.Settings
{
    public class DbSettings
    {
        public StorageMode StorageMode { get; set; }
        public string LogsConnString { get; set; }
        public string SnapshotsConnectionString { get; set; }
    }
}
