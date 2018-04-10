using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.CandlesProducer
{
    public class MtDataReaderClientSettings
    {
        [HttpCheck("/api/isalive")] 
        public string ServiceUrl { get; set; }
        public string ApiKey { get; set; }
    }
}
