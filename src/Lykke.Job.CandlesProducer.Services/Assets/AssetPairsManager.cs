using System.Threading.Tasks;
using Lykke.Job.CandlesProducer.Core.Services.Assets;
using Lykke.Service.Assets.Client;
using Lykke.Service.Assets.Client.Models;

namespace Lykke.Job.CandlesProducer.Services.Assets
{
    public class AssetPairsManager : IAssetPairsManager
    {
        private readonly IAssetsServiceWithCache _apiService;

        public AssetPairsManager(IAssetsServiceWithCache apiService)
        {
            _apiService = apiService;
        }

        public async Task<AssetPair> TryGetEnabledPairAsync(string assetPairId)
        {
            var pair = await _apiService.TryGetAssetPairAsync(assetPairId);

            return pair == null || pair.IsDisabled ? null : pair;
        }
    }
}
