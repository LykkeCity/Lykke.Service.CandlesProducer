using System;

namespace Lykke.Job.CandlesProducer.Core.Domain
{
    public class AssetPair
    {
        public AssetPair(string id, string baseAssetId, int accuracy)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException(nameof(id));
            
            if (string.IsNullOrWhiteSpace(baseAssetId))
                throw new ArgumentException(nameof(baseAssetId));
            
            if (accuracy < 0)
                throw new ArgumentException(nameof(accuracy));
            
            Id = id;
            BaseAssetId = baseAssetId;
            Accuracy = accuracy;
        }

        public string Id { get; }
        public string BaseAssetId { get; }
        public int Accuracy { get; }
    }
}
