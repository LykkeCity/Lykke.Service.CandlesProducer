﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using Common;
using Lykke.Job.CandlesProducer.Core.Domain.Candles;

namespace Lykke.Job.CandlesProducer.Core.Services.Candles
{
    public interface ICandlesPublisher : IStartable, IStopable
    {
        Task PublishAsync(IReadOnlyCollection<CandleUpdateResult> updates);
    }
}
