using Lykke.Job.CandlesProducer.Services;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.CandlesProducer.Settings
{
    public class CandlesPublicationRabbitSettings : IRabbitPublicationSettings
    {
        [AmqpCheck]
        public string ConnectionString { get; set; }
        public string Namespace { get; set; }
    }
}

