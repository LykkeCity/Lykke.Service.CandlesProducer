using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Lykke.Job.CandlesProducer.AzureRepositories;
using Lykke.Job.CandlesProducer.Core.Domain;
using Lykke.Job.CandlesProducer.Core.Domain.Candles;
using Newtonsoft.Json;
using Lykke.Logs.MsSql.Extensions;
using MarginTrading.SqlRepositories;
using Microsoft.Extensions.Internal;

namespace Lykke.Job.CandlesProducer.SqlRepositories
{
    public class SqlCandlesGeneratorSnapshotRepository : ISnapshotRepository<ImmutableDictionary<string, ICandle>>
    {
        private const string BlobContainer = "CandlesGeneratorSnapshot";
        private const string Key = "CandlesGenerator";
        private readonly ICandlesProducerBlobRepository _blobRepository;


        public SqlCandlesGeneratorSnapshotRepository(string connectionString)
        {
            _blobRepository = new SqlBlobRepository(connectionString);
        }

        public async Task<ImmutableDictionary<string, ICandle>> TryGetAsync()
        {
            var model = _blobRepository.Read<Dictionary<string, CandleEntity>>(BlobContainer, Key);
            if (model != null)
            {
                return model.ToImmutableDictionary(i => i.Key, i => (ICandle)i.Value);
            }
            return new Dictionary<string,ICandle>().ToImmutableDictionary();
        }

        public async Task SaveAsync(ImmutableDictionary<string, ICandle> state)
        {
            await _blobRepository.Write(BlobContainer, Key, state);
        }
    }
}
