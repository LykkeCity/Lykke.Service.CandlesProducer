using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Job.CandlesProducer.Core.Domain;
using Lykke.Job.CandlesProducer.Core.Services.Assets;
using MarginTrading.Backend.Contracts.DataReaderClient;
using Polly;
using Polly.Retry;

namespace Lykke.Job.CandlesProducer.Services.Assets
{
    public class MtAssetPairsManager : IAssetPairsManager
    {
        private readonly IMtDataReaderClient _mtDataReaderClient;
        private readonly RetryPolicy _retryPolicy;

        public MtAssetPairsManager(ILog log, IMtDataReaderClient mtDataReaderClient)
        {
            _mtDataReaderClient = mtDataReaderClient;
            _retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryForeverAsync(retryAttempt => TimeSpan.FromSeconds(Math.Min(60, Math.Pow(2, retryAttempt))),
                    (exception, timespan) =>
                        log.WriteErrorAsync("Get all mt asset pairs with retry", string.Empty, exception));
        }

        public async Task<AssetPair> TryGetEnabledPairAsync(string assetPairId)
        {
            return (await GetAllEnabledAsync()).FirstOrDefault(p => p.Id == assetPairId);
        }

        private Task<IEnumerable<AssetPair>> GetAllEnabledAsync()
        {
            // note the mtDataReaderClient caches the assetPairs for 3 minutes 
            return _retryPolicy.ExecuteAsync(async () =>
                (await _mtDataReaderClient.AssetPairsRead.List()).Select(p => new AssetPair(p.Id, p.BaseAssetId, p.Accuracy)));
        }
    }
}
