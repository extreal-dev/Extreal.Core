using System;

namespace Extreal.Core.Common.Retry
{
    /// <summary>
    /// The retry strategy to control retry processing by retry count.
    /// </summary>
    /// <remarks>
    /// The default is the Fibonacci number with a maximum of 20 retry processing.
    /// </remarks>
    public class CountingRetryStrategy : IRetryStrategy
    {
        private static readonly int[] FibonacciNumbers =
        {
            0, 1, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89, 144, 233, 377, 610, 987, 1597, 2584, 4181
        };

        private readonly int maxRetryCount;
        private readonly Func<int, TimeSpan> nextRetryInterval;

        private int retryCount;

        /// <summary>
        /// Creates a new CountingRetryStrategy.
        /// </summary>
        /// <param name="maxRetryCount">Max retry count.</param>
        /// <param name="nextRetryInterval">Processing to provide retry intervals.</param>
        /// <exception cref="ArgumentOutOfRangeException">If maxRetryCount is less than 1. Or if nextRetryInterval is greater than 20 without specifying nextRetryInterval.</exception>
        public CountingRetryStrategy(int maxRetryCount = 12, Func<int, TimeSpan> nextRetryInterval = null)
        {
            if (maxRetryCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxRetryCount), maxRetryCount, "Please specify 1 or more");
            }

            if (nextRetryInterval == null && FibonacciNumbers.Length < maxRetryCount)
            {
                throw new ArgumentOutOfRangeException(nameof(maxRetryCount), maxRetryCount,
                    $"The default for {nameof(this.nextRetryInterval)} is to use {FibonacciNumbers.Length} Fibonacci numbers, " +
                    $"so {nameof(maxRetryCount)} must be less than or equal to {FibonacciNumbers.Length}. " +
                    $"Alternatively, specify {nameof(nextRetryInterval)}");
            }

            this.maxRetryCount = maxRetryCount;
            this.nextRetryInterval = nextRetryInterval ?? (i => TimeSpan.FromSeconds(FibonacciNumbers[i - 1]));
        }

        /// <inheritdoc />
        public void Reset() => retryCount = 0;

        /// <inheritdoc />
        public bool HasNext() => retryCount < maxRetryCount;

        /// <inheritdoc />
        public TimeSpan Next()
        {
            retryCount++;
            return nextRetryInterval(retryCount);
        }
    }
}
