using System;
using Extreal.Core.Logging;

namespace Extreal.Core.Common.System
{
    /// <summary>
    /// Class for safely disposing.
    /// </summary>
    /// <remarks>
    /// This class provides the logic required for the the Dispose Pattern.
    /// By using this class, the Dispose Pattern can be implemented by delegation rather than inheritance.
    /// See <see cref="DisposableBase"/> for an implementation example.
    /// <para>
    /// Dispose Pattern:
    /// https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose
    /// </para>
    /// </remarks>
    public class SafeDisposer
    {
        private static readonly ELogger Logger = LoggingManager.GetLogger(nameof(SafeDisposer));

        private bool isDisposed;

        private readonly object target;
        private readonly Action releaseManagedResources;
        private readonly Action releaseUnmanagedResources;

        /// <summary>
        /// Creates a new SafeDisposer.
        /// </summary>
        /// <param name="target">Target object to dispose.</param>
        /// <param name="releaseManagedResources">Processing to release managed resources.</param>
        /// <param name="releaseUnmanagedResources">Processing to release unmanaged resources.</param>
        /// <exception cref="ArgumentException">If both releaseManagedResources and releaseUnmanagedResources are not specified.</exception>
        /// <exception cref="ArgumentNullException">If target is not specified.</exception>
        public SafeDisposer(object target, Action releaseManagedResources = null, Action releaseUnmanagedResources = null)
        {
            if (releaseManagedResources == null && releaseUnmanagedResources == null)
            {
                throw new ArgumentException(
                    $"Either {nameof(releaseManagedResources)} or {nameof(releaseUnmanagedResources)} is required");
            }

            this.target = target ?? throw new ArgumentNullException(nameof(target));
            this.releaseManagedResources = releaseManagedResources;
            this.releaseUnmanagedResources = releaseUnmanagedResources;
        }

        /// <summary>
        /// Disposes by Finalizer.
        /// </summary>
        public void DisposeByFinalizer() => Dispose(false);

        /// <summary>
        /// Disposes.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(target);
        }

        private void Dispose(bool disposing)
        {
            if (isDisposed)
            {
                return;
            }

            if (Logger.IsDebug())
            {
                Logger.LogDebug($"Dispose {target.GetType().Name}");
            }

            if (disposing)
            {
                releaseManagedResources?.Invoke();
                releaseUnmanagedResources?.Invoke();
            }
            else
            {
                releaseUnmanagedResources?.Invoke();
            }
            isDisposed = true;
        }
    }
}
