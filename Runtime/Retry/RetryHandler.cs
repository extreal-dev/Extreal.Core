using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Cysharp.Threading.Tasks;
using Extreal.Core.Common.System;
using Extreal.Core.Logging;
using UniRx;

namespace Extreal.Core.Common.Retry
{
    /// <summary>
    /// Class for controlling retry processing.
    /// </summary>
    /// <typeparam name="TResult">Result of retry target method</typeparam>
    public class RetryHandler<TResult> : DisposableBase
    {
        [SuppressMessage("Usage", "CC0033")]
        private readonly CompositeDisposable disposables = new CompositeDisposable();

        /// <summary>
        /// <para>Invokes just before retrying.</para>
        /// Arg: Retry count
        /// </summary>
        public IObservable<int> OnRetrying => onRetrying.AddTo(disposables);
        [SuppressMessage("Usage", "CC0033")]
        private readonly Subject<int> onRetrying = new Subject<int>();

        /// <summary>
        /// <para>Invokes immediately after finishing retrying.</para>
        /// Arg: Final results of retry. True for success, false for failure.
        /// </summary>
        public IObservable<bool> OnRetried => onRetried.AddTo(disposables);
        [SuppressMessage("Usage", "CC0033")]
        private readonly Subject<bool> onRetried = new Subject<bool>();

        private static readonly ELogger Logger = LoggingManager.GetLogger(nameof(RetryHandler<TResult>));

        private readonly Func<UniTask<TResult>> runAsync;
        private readonly Func<Exception, bool> isRetryable;
        private readonly IRetryStrategy retryStrategy;
        private readonly CancellationToken cancellationToken;

        private RetryHandler(
            Func<UniTask<TResult>> runAsync, Func<Exception, bool> isRetryable,
            IRetryStrategy retryStrategy, CancellationToken cancellationToken)
        {
            this.runAsync = runAsync ?? throw new ArgumentNullException(nameof(runAsync));
            this.isRetryable = isRetryable ?? throw new ArgumentNullException(nameof(isRetryable));
            this.retryStrategy = retryStrategy ?? throw new ArgumentNullException(nameof(retryStrategy));
            this.cancellationToken = cancellationToken;
        }

        /// <summary>
        /// Runs a target method for retry.
        /// </summary>
        /// <returns>Result of retry target method</returns>
        /// <exception cref="OperationCanceledException">If the retry process is canceled.</exception>
        public async UniTask<TResult> HandleAsync()
        {
            var retryCount = 0;
            retryStrategy.Reset();
            while (true)
            {
                try
                {
                    var result = await runAsync.Invoke();
                    FireOnRetriedIfRetryStarted(retryCount, true);
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
                            throw new OperationCanceledException("The retry was canceled", oce);
                        }

                        retryCount++;
                        FireOnRetrying(retryCount);
                    }
                    else
                    {
                        FireOnRetriedIfRetryStarted(retryCount, false);
                        throw;
                    }
                }
            }
        }

        private void FireOnRetrying(int retryCount)
        {
            if (Logger.IsDebug())
            {
                if (retryCount == 1)
                {
                    Logger.LogDebug(
                        $"RETRY START: run:{runAsync.Method.Name} isRetryable:{isRetryable.Method.Name} retryStrategy:{retryStrategy}");
                }
                Logger.LogDebug($"RETRY: retryCount:{retryCount}");
            }
            onRetrying.OnNext(retryCount);
        }

        private void FireOnRetriedIfRetryStarted(int retryCount, bool result)
        {
            if (retryCount == 0)
            {
                return;
            }
            if (Logger.IsDebug())
            {
                Logger.LogDebug($"RETRY FINISHED: result:{result}");
            }
            onRetried.OnNext(result);
        }

        private static void LogException(Exception e)
        {
            if (Logger.IsDebug())
            {
                Logger.LogDebug("Exception occurred!", e);
            }
        }

        /// <inheritdoc/>
        protected override void ReleaseManagedResources() => disposables.Dispose();

        /// <summary>
        /// Creates a new RetryHandler.
        /// </summary>
        /// <param name="action">Retry target method.</param>
        /// <param name="isRetryable">Processing used to determine retry.</param>
        /// <param name="retryStrategy">Strategy to control retry processing.</param>
        /// <param name="cancellationToken">Token used to cancel retry.</param>
        /// <returns>None</returns>
        [SuppressMessage("Usage", "CC0001")]
        public static RetryHandler<Unit> Of(
            Action action, Func<Exception, bool> isRetryable,
            IRetryStrategy retryStrategy, CancellationToken cancellationToken = default)
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
            return new RetryHandler<Unit>(runAsync, isRetryable, retryStrategy, cancellationToken);
        }

        /// <summary>
        /// Creates a new RetryHandler.
        /// </summary>
        /// <param name="actionAsync">Retry target method.</param>
        /// <param name="isRetryable">Processing used to determine retry.</param>
        /// <param name="retryStrategy">Strategy to control retry processing.</param>
        /// <param name="cancellationToken">Token used to cancel retry.</param>
        /// <returns>None</returns>
        [SuppressMessage("Usage", "CC0001")]
        public static RetryHandler<Unit> Of(
            Func<UniTask> actionAsync, Func<Exception, bool> isRetryable,
            IRetryStrategy retryStrategy, CancellationToken cancellationToken = default)
        {
            Func<UniTask<Unit>> runAsync = actionAsync != null
                ? async () =>
                {
                    await actionAsync();
                    return Unit.Default;
                }
                : null;
            return new RetryHandler<Unit>(runAsync, isRetryable, retryStrategy, cancellationToken);
        }

        /// <summary>
        /// Creates a new RetryHandler.
        /// </summary>
        /// <param name="func">Retry target method.</param>
        /// <param name="isRetryable">Processing used to determine retry.</param>
        /// <param name="retryStrategy">Strategy to control retry processing.</param>
        /// <param name="cancellationToken">Token used to cancel retry.</param>
        /// <typeparam name="T">Result of retry target method</typeparam>
        /// <returns>Result of retry target method</returns>
        [SuppressMessage("Usage", "CC0001")]
        public static RetryHandler<T> Of<T>(
            Func<T> func, Func<Exception, bool> isRetryable,
            IRetryStrategy retryStrategy, CancellationToken cancellationToken = default)
        {
#pragma warning disable CS1998
            Func<UniTask<T>> runAsync = func != null ? () => UniTask.Create(async () => func()) : null;
#pragma warning restore CS1998
            return new RetryHandler<T>(runAsync, isRetryable, retryStrategy, cancellationToken);
        }

        /// <summary>
        /// Creates a new RetryHandler.
        /// </summary>
        /// <param name="funcAsync">Retry target method.</param>
        /// <param name="isRetryable">Processing used to determine retry.</param>
        /// <param name="retryStrategy">Strategy to control retry processing.</param>
        /// <param name="cancellationToken">Token used to cancel retry.</param>
        /// <typeparam name="T">Result of retry target method</typeparam>
        /// <returns>Result of retry target method</returns>
        public static RetryHandler<T> Of<T>(
            Func<UniTask<T>> funcAsync, Func<Exception, bool> isRetryable,
            IRetryStrategy retryStrategy, CancellationToken cancellationToken = default)
            => new RetryHandler<T>(funcAsync, isRetryable, retryStrategy, cancellationToken);
    }
}
