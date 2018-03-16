using System;
using System.Reflection;
using JetBrains.Annotations;

namespace Lykke.Job.CandlesProducer.Contract
{
    /// <summary>
    /// Contract constants
    /// </summary>
    [PublicAPI]
    public static class Constants
    {
        /// <summary>
        /// Semver compatible contract version, but only major and minor parts are used
        /// </summary>
        public static readonly Version ContractVersion = new Version(4, 1, 1);
    }
}
