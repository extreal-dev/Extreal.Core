namespace Extreal.Core.Common.System
{
    /// <summary>
    /// Base class that implements the Dispose pattern.
    /// </summary>
    /// <remarks>
    /// This class can be used to implement the Dispose pattern by inheritance.
    /// If you want to implement the Dispose Pattern by delegation instead of inheritance, use <see cref="SafeDisposer"/>.
    /// <para>
    /// Dispose Pattern:
    /// https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose
    /// </para>
    /// </remarks>
    public class DisposableBase
    {
        private readonly SafeDisposer safeDisposer;

        /// <summary>
        /// Create a new DisposableBase.
        /// </summary>
        protected DisposableBase()
            => safeDisposer = new SafeDisposer(this, ReleaseManagedResources, ReleaseUnmanagedResources);

        ~DisposableBase() => safeDisposer.DisposeByFinalizer();

        /// <summary>
        /// Release managed resources.
        /// </summary>
        protected virtual void ReleaseManagedResources() { }

        /// <summary>
        /// Release unmanaged resources.
        /// </summary>
        protected virtual void ReleaseUnmanagedResources() { }

        public void Dispose() => safeDisposer.Dispose();
    }
}
