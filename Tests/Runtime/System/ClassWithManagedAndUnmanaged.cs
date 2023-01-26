namespace Extreal.Core.Common.System.Test
{
    public class ClassWithManagedAndUnmanaged : DisposableBase
    {
        public static void ResetTimes() => ManagedTimes = UnmanagedTimes = 0;

        public static int ManagedTimes { get; private set; }
        public static int UnmanagedTimes { get; private set; }

        protected override void ReleaseManagedResources() => ManagedTimes++;
        protected override void ReleaseUnmanagedResources() => UnmanagedTimes++;
    }
}
