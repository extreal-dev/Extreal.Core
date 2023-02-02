using System;

namespace Extreal.Core.Common.Retry
{
    public class NoRetryStrategy : IRetryStrategy
    {
        public void Reset() { }

        public bool HasNext() => false;

        public TimeSpan Next() => throw new InvalidOperationException("Unreachable");
    }
}
