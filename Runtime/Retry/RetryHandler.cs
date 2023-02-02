using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Cysharp.Threading.Tasks;
using Extreal.Core.Common.System;
using Extreal.Core.Logging;
using UniRx;

namespace Extreal.Core.Common.Retry
{
    public class RetryHandler<TResult> : DisposableBase
    {
        [SuppressMessage("Usage", "CC0033")]
        private readonly CompositeDisposable disposables = new CompositeDisposable();

        public IObservable<int> OnRetrying => onRetrying.AddTo(disposables);
        [SuppressMessage("Usage", "CC0033")] private readonly Subject<int> onRetrying = new Subject<int>();

        public IObservable<bool> OnRetried => onRetried.AddTo(disposables);
        [SuppressMessage("Usage", "CC0033")] private readonly Subject<bool> onRetried = new Subject<bool>();

        private static readonly ELogger Logger = LoggingManager.GetLogger(nameof(RetryHandler<TResult>));

        private readonly Func<UniTask<TResult>> runAsync;
        private readonly Func<Exception, bool> isRetryable;
        private readonly IRetryStrategy retryStrategy;

        private static readonly IRetryStrategy NoRetryStrategy = new NoRetryStrategy();

        private RetryHandler(
            Func<UniTask<TResult>> runAsync, Func<Exception, bool> isRetryable, IRetryStrategy retryStrategy = null)
        {
            this.runAsync = runAsync ?? throw new ArgumentNullException(nameof(runAsync));
            this.isRetryable = isRetryable ?? throw new ArgumentNullException(nameof(isRetryable));
            this.retryStrategy = retryStrategy ?? NoRetryStrategy;
        }

        public async UniTask<TResult> HandleAsync(CancellationToken cancellationToken = default)
        {
            var retryCount = 0;
            retryStrategy.Reset();
            while (true)
            {
                try
                {
                    var result = await runAsync.Invoke();
                    onRetried.OnNext(true);
                    return result;
                }
                catch (Exception e)
                {
                    LogException(e);
                    if (retryStrategy.HasNext() && isRetryable(e))
                    {
                        try
                        {
                            await UniTask.Delay(retryStrategy.Next(), cancellationToken: cancellationToken);
                        }
                        catch (OperationCanceledException oce)
                        {
                            LogException(e);
                            throw new OperationCanceledException("The retry was canceled", oce);
                        }

                        retryCount++;
                        onRetrying.OnNext(retryCount);
                        LogRetry(retryCount);
                    }
                    else
                    {
                        onRetried.OnNext(false);
                        throw;
                    }
                }
            }
        }

        private void LogRetry(int retryCount)
        {
            if (Logger.IsDebug())
            {
                Logger.LogDebug(
                    $"retry: {retryCount} run: {runAsync} isRetryable: {isRetryable} retryStrategy: {retryStrategy}");
            }
        }

        private static void LogException(Exception e)
        {
            if (Logger.IsDebug())
            {
                Logger.LogDebug("Exception occurred!", e);
            }
        }

        protected override void ReleaseManagedResources() => disposables.Dispose();


        [SuppressMessage("Usage", "CC0001")]
        public static RetryHandler<Unit> Of(
            Action action, Func<Exception, bool> isRetryable, IRetryStrategy retryStrategy = null)
        {
#pragma warning disable CS1998
            Func<UniTask<Unit>> runAsync = action != null
                ? () => UniTask.Create(async () =>
                {
                    action();
                    return Unit.Default;
                })
                : null;
#pragma warning restore CS1998
            return new RetryHandler<Unit>(runAsync, isRetryable, retryStrategy);
        }

        [SuppressMessage("Usage", "CC0001")]
        public static RetryHandler<Unit> Of(
            Func<UniTask> actionAsync, Func<Exception, bool> isRetryable, IRetryStrategy retryStrategy = null)
        {
            Func<UniTask<Unit>> runAsync = actionAsync != null
                ? async () =>
                {
                    await actionAsync();
                    return Unit.Default;
                }
                : null;
            return new RetryHandler<Unit>(runAsync, isRetryable, retryStrategy);
        }

        [SuppressMessage("Usage", "CC0001")]
        public static RetryHandler<T> Of<T>(
            Func<T> func, Func<Exception, bool> isRetryable, IRetryStrategy retryStrategy = null)
        {
#pragma warning disable CS1998
            Func<UniTask<T>> runAsync = func != null ? () => UniTask.Create(async () => func()) : null;
#pragma warning restore CS1998
            return new RetryHandler<T>(runAsync, isRetryable, retryStrategy);
        }

        public static RetryHandler<T> Of<T>(
            Func<UniTask<T>> funcAsync, Func<Exception, bool> isRetryable, IRetryStrategy retryStrategy = null)
            => new RetryHandler<T>(funcAsync, isRetryable, retryStrategy);
    }
}
