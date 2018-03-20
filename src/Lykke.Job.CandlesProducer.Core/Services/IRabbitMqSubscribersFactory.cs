using System;
using System.Threading.Tasks;
using Common;

namespace Lykke.Job.CandlesProducer.Core.Services
{
    public interface IRabbitMqSubscribersFactory
    {
        IStopable Create<TMessage>(string connectionString, string @namespace, string source, Func<TMessage, Task> handler, string queueSuffix = null);
    }
}
