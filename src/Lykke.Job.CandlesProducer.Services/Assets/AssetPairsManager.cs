using System.Threading.Tasks;
using Lykke.Job.CandlesProducer.Core.Domain;
using Lykke.Job.CandlesProducer.Core.Services.Assets;
using Lykke.Service.Assets.Client.Custom;

namespace Lykke.Job.CandlesProducer.Services.Assets
{
    public class AssetPairsManager : IAssetPairsManager
    {
        private readonly ICachedAssetsService _apiService;

        public AssetPairsManager(ICachedAssetsService apiService)
        {
            _apiService = apiService;
        }

        public async Task<AssetPair> TryGetEnabledPairAsync(string assetPairId)
        {
            var pair = await _apiService.TryGetAssetPairAsync(assetPairId);

            return pair == null || pair.IsDisabled ? null : new AssetPair(pair.Id, pair.BaseAssetId, pair.Accuracy);
        }
    }
}
