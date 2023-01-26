namespace Extreal.Core.Common.System.Test
{
    public class ClassWithUnmanaged : DisposableBase
    {
        public static void ResetTimes() => UnmanagedTimes = 0;

        public static int UnmanagedTimes { get; private set; }

        protected override void ReleaseUnmanagedResources() => UnmanagedTimes++;
    }
}
