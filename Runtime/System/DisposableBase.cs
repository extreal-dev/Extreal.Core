using System;
using System.Diagnostics.CodeAnalysis;

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
    public class DisposableBase : IDisposable
    {
        private readonly SafeDisposer safeDisposer;

        /// <summary>
        /// Creates a new DisposableBase.
        /// </summary>
        protected DisposableBase()
            => safeDisposer = new SafeDisposer(this, ReleaseManagedResources, ReleaseUnmanagedResources);

        ~DisposableBase() => safeDisposer.DisposeByFinalizer();

        /// <summary>
        /// Releases managed resources.
        /// </summary>
        protected virtual void ReleaseManagedResources() { }

        /// <summary>
        /// Releases unmanaged resources.
        /// </summary>
        protected virtual void ReleaseUnmanagedResources() { }

        [SuppressMessage("Usage", "CC0029")]
        public void Dispose() => safeDisposer.Dispose();
    }
}
