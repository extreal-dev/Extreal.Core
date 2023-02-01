using System;
using System.Diagnostics.CodeAnalysis;
using Cysharp.Threading.Tasks;
using Extreal.Core.Logging;

namespace Extreal.Core.Common.Retry.Test
{
    public class ClassWithRetry
    {
        private static readonly ELogger Logger = LoggingManager.GetLogger(nameof(ClassWithRetry));

        private readonly int failureCount;

        public int RunCount => runCount;
        private int runCount;

        public int ThrowCount => throwCount;
        private int throwCount;

        public void Reset() => runCount = throwCount = 0;

        [SuppressMessage("Usage", "CC0057")]
        public ClassWithRetry(int failureCount) => this.failureCount = failureCount;

        public void RunAction() => Run();

        public async UniTask RunActionAsync() => await RunAsync();

        public string RunFunc() => Run();

        public async UniTask<string> RunFuncAsync() => await RunAsync();

        private string Run()
        {
            runCount++;

            if (Logger.IsDebug())
            {
                Logger.LogDebug($"RUN runCount:{runCount} failureCount:{failureCount}");
            }

            if (failureCount != 0 && runCount <= failureCount)
            {
                throwCount++;
                throw new AccessViolationException("THROW RETRY TEST");
            }
            else
            {
                return "RETURN RETRY TEST";
            }
        }

#pragma warning disable CS1998
        private async UniTask<string> RunAsync() => await UniTask.Create(async () => Run());
#pragma warning restore CS1998
    }
}
