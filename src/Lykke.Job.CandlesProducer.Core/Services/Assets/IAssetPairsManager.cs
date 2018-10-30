using System.Threading.Tasks;
using Lykke.Service.Assets.Client.Models;

namespace Lykke.Job.CandlesProducer.Core.Services.Assets
{
    public interface IAssetPairsManager
    {
        Task<AssetPair> TryGetEnabledPairAsync(string assetPairId);
    }
}
