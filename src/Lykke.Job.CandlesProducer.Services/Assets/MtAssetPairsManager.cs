using System.Threading.Tasks;
using Lykke.Job.CandlesProducer.Core.Services.Assets;
using Lykke.Service.Assets.Client.Custom;
using MarginTrading.SettingsService.Contracts;

namespace Lykke.Job.CandlesProducer.Services.Assets
{
    public class MtAssetPairsManager : IAssetPairsManager
    {
        private readonly IAssetPairsApi _apiService;

        public MtAssetPairsManager(IAssetPairsApi apiService)
        {
            _apiService = apiService;
        }

        public async Task<IAssetPair> TryGetEnabledPairAsync(string assetPairId)
        {
            var pair = await _apiService.Get(assetPairId);

            return pair == null ? null : new AssetPair {
                                              Id = pair.Id,
                                              Name = pair.Name,
                                              BaseAssetId = pair.BaseAssetId,
                                              QuotingAssetId = pair.QuoteAssetId,
                                              Accuracy = pair.Accuracy,
                                              InvertedAccuracy = pair.Accuracy
                                        };
        }
    }
}
