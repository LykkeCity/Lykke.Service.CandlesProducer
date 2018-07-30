using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Lykke.Job.CandlesProducer.Core.Domain;
using Lykke.Logs.MsSql.Extensions;
using Newtonsoft.Json;

namespace MarginTrading.SqlRepositories
{
    public class SqlBlobRepository : ICandlesProducerBlobRepository
    {
        private const string TableName = "BlobData";
        private const string CreateTableScript = "CREATE TABLE [{0}](" +
                                                 "[BlobKey] [nvarchar] (64) NOT NULL PRIMARY KEY, " +
                                                 "[Data] [nvarchar] (MAX) NULL, " +
                                                 "[Timestamp] [DateTime] NULL " +
                                                 ");";

        private readonly string _connectionString;

        public SqlBlobRepository(string connectionString)
        {
            _connectionString = connectionString;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.CreateTableIfDoesntExists(CreateTableScript, TableName);
            }
        }

        public T Read<T>(string blobContainer, string key)
        {
            return ReadAsync<T>(blobContainer, key).GetAwaiter().GetResult();
        }

        public async Task<T> ReadAsync<T>(string blobContainer, string key)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var data = (await conn.QueryAsync<string>(
                    $"SELECT Data FROM {TableName} WHERE BlobKey=@blobKey",
                    new { blobKey = $"{blobContainer}_{key}" })).SingleOrDefault();

                if (string.IsNullOrEmpty(data) || data == "{}" )
                    return default(T);

                var settings = new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.All
                };
                return JsonConvert.DeserializeObject<T>(data);
            }
        }

        public async Task Write<T>(string blobContainer, string key, T obj)
        {
            var request = new
            {
                data = JsonConvert.SerializeObject(obj),
                blobKey = $"{blobContainer}_{key}",
                timestamp = DateTime.Now
            };

            using (var conn = new SqlConnection(_connectionString))
            {
                try
                {
                    await conn.ExecuteAsync(
                        $"insert into {TableName} (BlobKey, Data, Timestamp) values (@blobKey, @data, @timestamp)",
                        request);
                }
                catch
                {
                    await conn.ExecuteAsync(
                        $"update {TableName} set Data=@data, Timestamp = @timestamp where BlobKey=@blobKey",
                        request);
                }
            }
        }
    }
}
