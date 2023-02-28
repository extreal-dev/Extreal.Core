using System;

namespace Extreal.Core.Common.Retry
{
    /// <summary>
    /// The retry strategy without retry.
    /// </summary>
    /// <remarks>
    /// Since NoRetryStrategy is a singleton, it is referenced from the class variable Instance.
    /// </remarks>
    public class NoRetryStrategy : IRetryStrategy
    {
        /// <summary>
        /// The NoRetryStrategy instance.
        /// </summary>
        public static readonly IRetryStrategy Instance = new NoRetryStrategy();

        private NoRetryStrategy() { }

        /// <summary>
        /// Does nothing.
        /// </summary>
        public void Reset() { }

        /// <summary>
        /// Returns always false.
        /// </summary>
        /// <returns>false</returns>
        public bool HasNext() => false;

        /// <summary>
        /// Throws the InvalidOperationException.
        /// </summary>
        /// <returns>None</returns>
        /// <exception cref="InvalidOperationException">If this method is called.</exception>
        public TimeSpan Next() => throw new InvalidOperationException("Unreachable");
    }
}
