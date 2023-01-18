using System;

namespace Extreal.Core.System
{
    public abstract class DisposableBase : IDisposable
    {
        private bool isDisposed;

        protected abstract void FreeResources();

        ~DisposableBase()
        {
            DisposeInternal();
        }

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
