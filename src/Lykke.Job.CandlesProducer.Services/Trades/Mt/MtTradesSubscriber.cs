using System.Threading.Tasks;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Job.CandlesProducer.Core.Domain.Trades;
using Lykke.Job.CandlesProducer.Core.Services;
using Lykke.Job.CandlesProducer.Core.Services.Candles;
using Lykke.Job.CandlesProducer.Core.Services.Trades;
using Lykke.Job.CandlesProducer.Services.Trades.Mt.Messages;

namespace Lykke.Job.CandlesProducer.Services.Trades.Mt
{
    [UsedImplicitly]
    public class MtTradesSubscriber : ITradesSubscriber
    {
        private readonly ILog _log;
        private readonly ICandlesManager _candlesManager;
        private readonly IRabbitMqSubscribersFactory _subscribersFactory;
        private readonly string _connectionString;
        private IStopable _tradesSubscriber;

        public MtTradesSubscriber(ILogFactory logFactory, ICandlesManager candlesManager, IRabbitMqSubscribersFactory subscribersFactory, string connectionString)
        {
            _log = logFactory.CreateLog(this);
            _candlesManager = candlesManager;
            _subscribersFactory = subscribersFactory;
            _connectionString = connectionString;
        }

        public void Start()
        {
            _tradesSubscriber = _subscribersFactory.Create<MtTradeMessage>(_connectionString, "lykke.mt", "trades", ProcessTradeAsync, "-v2");
        }

        private async Task ProcessTradeAsync(MtTradeMessage message)
        {
            // Just discarding trades with negative or zero prices and\or volumes.
            if (message.Price <= 0 ||
                message.Volume <= 0)
            {
                _log.Warning(nameof(ProcessTradeAsync), "Got an MT trade with non-positive price or volume value.", context: message.ToJson());
                return;
            }

            var quotingVolume = (double) (message.Volume * message.Price);

            var trade = new Trade(
                message.AssetPairId,
                message.Date,
                (double) message.Volume,
                quotingVolume,
                (double) message.Price);

            await _candlesManager.ProcessTradeAsync(trade);
        }

        public void Dispose()
        {
            _tradesSubscriber?.Dispose();
        }

        public void Stop()
        {
            _tradesSubscriber?.Stop();
        }
    }
}
