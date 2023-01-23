using System;

namespace Extreal.Core
{
    /// <summary>
    /// Base class for classes with unmanaged resources.
    /// </summary>
    public abstract class DisposableBase : IDisposable
    {
        private bool isDisposed;

        /// <summary>
        /// Releases unmanaged resources and disposes managed resources.
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
