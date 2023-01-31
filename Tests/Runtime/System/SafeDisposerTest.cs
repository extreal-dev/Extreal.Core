using System;
using NUnit.Framework;

namespace Extreal.Core.Common.System.Test
{
    public class SafeDisposerTest
    {
        [Test]
        public void TargetIsNull() =>
            Assert.That(() => _ = new SafeDisposer(null, () => { }, () => { }),
                Throws.TypeOf<ArgumentNullException>().With.Message.
                    EqualTo($"Value cannot be null.{Environment.NewLine}Parameter name: target"));

        [Test]
        public void BothReleaseManagedResourcesAndReleaseUnmanagedResourcesAreNull() =>
            Assert.That(() => _ = new SafeDisposer(new object(), null, null),
                Throws.TypeOf<ArgumentException>().With.Message
                    .EqualTo("Either releaseManagedResources or releaseUnmanagedResources is required"));
    }
}
