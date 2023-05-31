using System;
using Extreal.Core.Logging;
using UniRx;

namespace Extreal.Core.Common.Hook
{
    /// <summary>
    /// Class for extending Observable.
    /// </summary>
    public static class ObservableExtensions
    {
        private static readonly ELogger Logger = LoggingManager.GetLogger(nameof(ObservableExtensions));

        /// <summary>
        /// Hooks a notification.
        /// </summary>
        /// <remarks>
        /// The hook must be implemented in such a way that no exception is raised.
        /// If an exception is raised by the hook, nothing is done because it does not affect the processing of other Subscribers.
        /// </remarks>
        /// <param name="source">Observable</param>
        /// <param name="hook">Processing for hook</param>
        /// <typeparam name="T">Type of value to be notified</typeparam>
        /// <returns>Observable</returns>
        /// <exception cref="ArgumentNullException">If hook is null</exception>
        public static IDisposable Hook<T>(this IObservable<T> source, Action<T> hook)
        {
            if (hook == null)
            {
                throw new ArgumentNullException(nameof(hook));
            }

            return source.Subscribe(value => InvokeHookSafely(hook, value));
        }

        private static void InvokeHookSafely<T>(Action<T> hook, T value)
        {
            try
            {
                hook.Invoke(value);
            }
            catch (Exception e)
            {
                if (Logger.IsDebug())
                {
                    // Output the log at the Error level so that the developer is aware of the error during development.
                    Logger.LogError("Error occurred on hook", e);
                }
            }
        }
    }
}
