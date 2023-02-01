using System;

namespace Extreal.Core.Common.Retry
{
    public interface IRetryStrategy
    {
        void Reset();
        bool HasNext();
        TimeSpan Next();
    }
}
