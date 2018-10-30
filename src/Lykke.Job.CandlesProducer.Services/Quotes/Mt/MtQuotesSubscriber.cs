using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Job.CandlesProducer.Core.Services;
using Lykke.Job.CandlesProducer.Core.Services.Candles;
using Lykke.Job.CandlesProducer.Core.Services.Quotes;
using Lykke.Job.CandlesProducer.Services.Quotes.Mt.Messages;
using Lykke.Job.QuotesProducer.Contract;

namespace Lykke.Job.CandlesProducer.Services.Quotes.Mt
{
    public class MtQuotesSubscriber : IQuotesSubscriber
    {
        private readonly ILog _log;
        private readonly ICandlesManager _candlesManager;
        private readonly IRabbitMqSubscribersFactory _subscribersFactory;
        private readonly string _connectionString;

        private IStopable _subscriber;

        public MtQuotesSubscriber(ILogFactory logFactory, ICandlesManager candlesManager, IRabbitMqSubscribersFactory subscribersFactory, string connectionString)
        {
            _log = logFactory.CreateLog(this);
            _candlesManager = candlesManager;
            _subscribersFactory = subscribersFactory;
            _connectionString = connectionString;
        }

        public void Start()
        {
            _subscriber = _subscribersFactory.Create<MtQuoteMessage>(_connectionString, "lykke.mt", "pricefeed", ProcessQuoteAsync);
        }

        public void Stop()
        {
            _subscriber?.Stop();
        }

        private async Task ProcessQuoteAsync(MtQuoteMessage quote)
        {
            try
            {
                var validationErrors = ValidateQuote(quote);
                if (validationErrors.Any())
                {
                    var message = string.Join("\r\n", validationErrors);
                    _log.Warning(nameof(ProcessQuoteAsync), message, context: quote.ToJson());

                    return;
                }

                if (quote.Bid > 0)
                {
                    var bidQuote = new QuoteMessage
                    {
                        AssetPair = quote.Instrument,
                        IsBuy = true,
                        Price = quote.Bid,
                        Timestamp = quote.Date
                    };

                    await _candlesManager.ProcessQuoteAsync(bidQuote);
                }
                else
                {
                    _log.Warning(nameof(ProcessQuoteAsync), "bid quote is skipped due to not positive price", context: quote.ToJson());
                }

                if (quote.Ask > 0)
                {
                    var askQuote = new QuoteMessage
                    {
                        AssetPair = quote.Instrument,
                        IsBuy = false,
                        Price = quote.Ask,
                        Timestamp = quote.Date
                    };

                    await _candlesManager.ProcessQuoteAsync(askQuote);
                }
                else
                {
                    _log.Warning(nameof(ProcessQuoteAsync), "bid quote is skipped due to not positive price", context: quote.ToJson());
                }
            }
            catch (Exception)
            {
                _log.Warning(nameof(ProcessQuoteAsync), "Failed to process quote", context: quote.ToJson());
                throw;
            }
        }

        private static IReadOnlyCollection<string> ValidateQuote(MtQuoteMessage quote)
        {
            var errors = new List<string>();

            if (quote == null)
            {
                errors.Add("Argument 'Order' is null.");
            }
            else
            {
                if (string.IsNullOrEmpty(quote.Instrument))
                {
                    errors.Add("Empty 'Instrument'");
                }
                if (quote.Date.Kind != DateTimeKind.Utc)
                {
                    errors.Add($"Invalid 'Date' Kind (UTC is required): '{quote.Date.Kind}'");
                }
            }

            return errors;
        }

        public void Dispose()
        {
            _subscriber?.Dispose();
        }
    }
}
