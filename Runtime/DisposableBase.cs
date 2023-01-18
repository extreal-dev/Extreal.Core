using System;

namespace Extreal.Core.System
{
    public abstract class DisposableBase : IDisposable
    {
        private bool isDisposed;

        protected virtual void FreeUnmanagedResources() { }
        protected virtual void FreeManagedResources() { }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (isDisposed)
            {
                return;
            }

            FreeUnmanagedResources();
            if (disposing)
            {
                FreeManagedResources();
            }

            isDisposed = true;
        }
    }
}
