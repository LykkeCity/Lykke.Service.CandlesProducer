using JetBrains.Annotations;
using Lykke.Service.Assets.Client.Custom;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.CandlesProducer.Settings
{
    public class AppSettings
    {        
        [Optional]
        public CandlesProducerSettings CandlesProducerJob { get; set; }
        [Optional]
        public CandlesProducerSettings MtCandlesProducerJob { get; set; }
        [Optional, CanBeNull]
        public SlackNotificationsSettings SlackNotifications { get; set; }
        public AssetsSettings Assets { get; set; }        
    }
}
