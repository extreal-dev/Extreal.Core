using System;
using System.Diagnostics.CodeAnalysis;
using Extreal.Core.Common.System;
using Extreal.Core.Logging;
using UniRx;

namespace Extreal.Core.Common.Retry
{
    public class RetryHandler : DisposableBase
    {
        public IObservable<int> OnRetrying => onRetrying.AddTo(disposables);
        [SuppressMessage("Usage", "CC0033")]
        private readonly Subject<int> onRetrying = new Subject<int>();

        [SuppressMessage("Usage", "CC0033")]
        private readonly CompositeDisposable disposables = new CompositeDisposable();

        private static readonly ELogger Logger = LoggingManager.GetLogger(nameof(RetryHandler));

        private readonly Func<object> run;
        private readonly Func<Exception, bool> isRetryable;
        private readonly IRetryStrategy retryStrategy;

        private static readonly IRetryStrategy NullRetryStrategy = new NullRetryStrategy();

        public RetryHandler(Func<object> run, Func<Exception, bool> isRetryable, IRetryStrategy retryStrategy)
        {
            this.run = run ?? throw new ArgumentNullException(nameof(run));
            this.isRetryable = isRetryable ?? throw new ArgumentNullException(nameof(isRetryable));
            this.retryStrategy = retryStrategy ?? NullRetryStrategy;
        }

        public object Handle()
        {
            var retryCount = 0;
            retryStrategy.Reset();
            while (true)
            {
                try
                {
                    return run;
                }
                catch (Exception e)
                {
                    if (Logger.IsDebug())
                    {
                        Logger.LogDebug("Exception occurred!", e);
                    }
                    if (retryStrategy.HasNext() && isRetryable(e))
                    {
                        retryStrategy.Wait();
                        retryCount++;
                        if (Logger.IsDebug())
                        {
                            Logger.LogDebug($"retry: {retryCount} run: {run} isRetryable: {isRetryable} retryStrategy: {retryStrategy}");
                        }
                        onRetrying.OnNext(retryCount);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        protected override void ReleaseManagedResources() => disposables.Dispose();
    }
}
