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
        /// Hooks the subscription.
        /// </summary>
        /// <remarks>
        /// Hook should be implemented in such a way that no exceptions are raised.
        /// To avoid affecting the processing of Subscribe, if an exception is raised by the hook, nothing is processed.
        /// </remarks>
        /// <param name="source">Observable</param>
        /// <param name="hook">Processing for hook</param>
        /// <typeparam name="T">Type of value to be notified</typeparam>
        /// <returns>Observable</returns>
        /// <exception cref="ArgumentNullException">If hook is null</exception>
        public static IObservable<T> Hook<T>(this IObservable<T> source, Action<T> hook)
        {
            if (hook == null)
            {
                throw new ArgumentNullException(nameof(hook));
            }

            return source.Do(obj => InvokeHookSafely(hook, obj));
        }

        private static void InvokeHookSafely<T>(Action<T> hook, T obj)
        {
            try
            {
                hook.Invoke(obj);
            }
            catch (Exception e)
            {
                if (Logger.IsDebug())
                {
                    Logger.LogDebug("Error occurred on hook", e);
                }
            }
        }
    }
}
