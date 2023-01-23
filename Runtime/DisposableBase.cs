using System;

namespace Extreal.Core
{
    /// <summary>
    /// <para>
    ///   Base class for classes with unmanaged resources.
    /// </para>
    /// <para>
    ///   The following processing is provided in the base class to ensure that the Dispose method is safe.
    ///   <list type="bullet">
    ///     <item>Even if the Dispose method is called multiple times, it is executed only once.</item>
    ///     <item>Call the Dispose method in the Finalizer.</item>
    ///     <item>Suppress the finalization of this instance.</item>
    ///   </list>
    /// </para>
    /// <para>
    ///   Inherit this Base class and implement resource release processing in the FreeResources method.
    ///   Implement the FreeResources method so that no exceptions are raised all resources are released.
    /// </para>
    /// </summary>
    public abstract class DisposableBase : IDisposable
    {
        private bool isDisposed;

        /// <summary>
        /// <para>Releases unmanaged resources and disposes managed resources.</para>
        /// <para>Implement so that no exceptions are raised and all resources are released.</para>
        /// </summary>
        protected abstract void FreeResources();

        ~DisposableBase()
        {
            DisposeInternal();
        }

        /// <summary>
        /// Disposes this instance.
        /// </summary>
        public void Dispose()
        {
            DisposeInternal();
            GC.SuppressFinalize(this);
        }

        private void DisposeInternal()
        {
            if (isDisposed)
            {
                return;
            }

            FreeResources();
            isDisposed = true;
        }
    }
}
