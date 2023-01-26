namespace Extreal.Core.Common.System.Test
{
    public class ClassWithManaged : DisposableBase
    {
        public static void ResetTimes() => ManagedTimes = 0;

        public static int ManagedTimes { get; private set; }

        protected override void ReleaseManagedResources() => ManagedTimes++;
    }
}
