using System.Collections.Generic;
using System.IO;

namespace Extreal.Core.System.Test
{
    public class DisposableSubClass : DisposableBase
    {
        public List<int> List { get; }

        public MemoryStream Stream { get; }

        public DisposableSubClass()
        {
            List = new List<int> { 0, 1, 2, 3, 4, 5 };
            Stream = new MemoryStream(65536);
        }

        ~DisposableSubClass()
            => Dispose(false);

        protected override void FreeUnmanagedResources()
            => Stream?.Dispose();

        protected override void FreeManagedResources()
            => List?.Clear();
    }
}
