using JetBrains.Annotations;
using Lykke.Sdk.Settings;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.CandlesProducer.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AppSettings : BaseAppSettings
    {        
        [Optional]
        public CandlesProducerSettings CandlesProducerJob { get; set; }
        [Optional]
        public CandlesProducerSettings MtCandlesProducerJob { get; set; }
        public AssetSettings Assets { get; set; }        
    }
}
