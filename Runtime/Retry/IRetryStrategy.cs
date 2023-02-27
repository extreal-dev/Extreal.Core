using System;

namespace Extreal.Core.Common.Retry
{
    /// <summary>
    /// The strategy to control retry processing.
    /// </summary>
    public interface IRetryStrategy
    {
        /// <summary>
        /// Resets retry status.
        /// </summary>
        /// <remarks>
        /// Reset is called when the retry processing is repeated.
        /// </remarks>
        void Reset();

        /// <summary>
        /// Determines whether to continue retry.
        /// </summary>
        /// <returns>True if retry continues, false otherwise.</returns>
        bool HasNext();

        /// <summary>
        /// Gets the retry interval.
        /// </summary>
        /// <returns>The retry interval.</returns>
        TimeSpan Next();
    }
}
