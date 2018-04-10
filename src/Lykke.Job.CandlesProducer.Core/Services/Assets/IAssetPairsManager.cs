using System.Threading.Tasks;
using Lykke.Job.CandlesProducer.Core.Domain;

namespace Lykke.Job.CandlesProducer.Core.Services.Assets
{
    public interface IAssetPairsManager
    {
        Task<AssetPair> TryGetEnabledPairAsync(string assetPairId);
    }
}
