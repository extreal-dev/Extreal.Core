using System;
using System.Collections;
using Extreal.Core.Logging;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Extreal.Core.Common.System.Test
{
    public class DisposableBaseTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp() => LoggingManager.Initialize(logLevel: LogLevel.Debug);

        [SetUp]
        public void SetUp()
        {
            ClassWithManaged.ResetTimes();
            ClassWithUnmanaged.ResetTimes();
            ClassWithManagedAndUnmanaged.ResetTimes();
        }

        [Test]
        public void DisposeForClassWithManaged()
        {
            var sut = new ClassWithManaged();
            sut.Dispose();
            Assert.That(ClassWithManaged.ManagedTimes, Is.EqualTo(1));
        }

        [Test]
        public void RepeatDisposeForClassWithManaged()
        {
            var sut = new ClassWithManaged();
            sut.Dispose();
            Assert.That(ClassWithManaged.ManagedTimes, Is.EqualTo(1));
            sut.Dispose();
            Assert.That(ClassWithManaged.ManagedTimes, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator FinalizerForClassWithManaged()
        {
            // Managedが呼ばれないことをテストしたいため、
            // 確実ではありませんが待つためにUnmanagedを使用
            _ = new ClassWithManaged();
            _ = new ClassWithUnmanaged();
            yield return null;

            GC.Collect();
            while (ClassWithUnmanaged.UnmanagedTimes == 0)
            {
                yield return null;
            }

            Assert.That(ClassWithManaged.ManagedTimes, Is.EqualTo(0));
            Assert.That(ClassWithUnmanaged.UnmanagedTimes, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator DisposeAndFinalizerForClassWithManaged()
        {
            // Managedが呼ばれないことをテストしたいため、
            // 確実ではありませんが待つためにUnmanagedを使用
            var sut1 = new ClassWithManaged();
            _ = new ClassWithUnmanaged();
            sut1.Dispose();
            Assert.That(ClassWithManaged.ManagedTimes, Is.EqualTo(1));
            Assert.That(ClassWithUnmanaged.UnmanagedTimes, Is.EqualTo(0));
            sut1 = null;
            yield return null;

            GC.Collect();
            while (ClassWithUnmanaged.UnmanagedTimes == 0)
            {
                yield return null;
            }

            Assert.That(ClassWithManaged.ManagedTimes, Is.EqualTo(1));
            Assert.That(ClassWithUnmanaged.UnmanagedTimes, Is.EqualTo(1));
        }

        [Test]
        public void DisposeForClassWithUnmanaged()
        {
            var sut = new ClassWithUnmanaged();
            sut.Dispose();
            Assert.That(ClassWithUnmanaged.UnmanagedTimes, Is.EqualTo(1));
        }

        [Test]
        public void RepeatDisposeForClassWithUnmanaged()
        {
            var sut = new ClassWithUnmanaged();
            sut.Dispose();
            Assert.That(ClassWithUnmanaged.UnmanagedTimes, Is.EqualTo(1));
            sut.Dispose();
            Assert.That(ClassWithUnmanaged.UnmanagedTimes, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator FinalizerForClassWithUnmanaged()
        {
            _ = new ClassWithUnmanaged();
            yield return null;

            GC.Collect();
            while (ClassWithUnmanaged.UnmanagedTimes == 0)
            {
                yield return null;
            }

            Assert.That(ClassWithUnmanaged.UnmanagedTimes, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator DisposeAndFinalizerForClassWithUnmanaged()
        {
            var sut = new ClassWithUnmanaged();
            sut.Dispose();
            sut = null;
            Assert.That(ClassWithUnmanaged.UnmanagedTimes, Is.EqualTo(1));
            yield return null;

            GC.Collect();
            while (ClassWithUnmanaged.UnmanagedTimes == 0)
            {
                yield return null;
            }

            Assert.That(ClassWithUnmanaged.UnmanagedTimes, Is.EqualTo(1));
        }

        [Test]
        public void DisposeForClassWithManagedAndUnmanaged()
        {
            var sut = new ClassWithManagedAndUnmanaged();
            sut.Dispose();
            Assert.That(ClassWithManagedAndUnmanaged.ManagedTimes, Is.EqualTo(1));
            Assert.That(ClassWithManagedAndUnmanaged.UnmanagedTimes, Is.EqualTo(1));
        }

        [Test]
        public void RepeatDisposeForClassWithManagedAndUnmanaged()
        {
            var sut = new ClassWithManagedAndUnmanaged();
            sut.Dispose();
            Assert.That(ClassWithManagedAndUnmanaged.ManagedTimes, Is.EqualTo(1));
            Assert.That(ClassWithManagedAndUnmanaged.UnmanagedTimes, Is.EqualTo(1));
            sut.Dispose();
            Assert.That(ClassWithManagedAndUnmanaged.ManagedTimes, Is.EqualTo(1));
            Assert.That(ClassWithManagedAndUnmanaged.UnmanagedTimes, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator FinalizerForClassWithManagedAndUnmanaged()
        {
            _ = new ClassWithManagedAndUnmanaged();
            yield return null;

            GC.Collect();
            while (ClassWithManagedAndUnmanaged.UnmanagedTimes == 0)
            {
                yield return null;
            }

            Assert.That(ClassWithManagedAndUnmanaged.ManagedTimes, Is.EqualTo(0));
            Assert.That(ClassWithManagedAndUnmanaged.UnmanagedTimes, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator DisposeAndFinalizerForClassWithManagedAndUnmanaged()
        {
            var sut = new ClassWithManagedAndUnmanaged();
            sut.Dispose();
            sut = null;
            Assert.That(ClassWithManagedAndUnmanaged.ManagedTimes, Is.EqualTo(1));
            Assert.That(ClassWithManagedAndUnmanaged.UnmanagedTimes, Is.EqualTo(1));
            yield return null;

            GC.Collect();
            while (ClassWithManagedAndUnmanaged.UnmanagedTimes == 0)
            {
                yield return null;
            }

            Assert.That(ClassWithManagedAndUnmanaged.ManagedTimes, Is.EqualTo(1));
            Assert.That(ClassWithManagedAndUnmanaged.UnmanagedTimes, Is.EqualTo(1));
        }
    }
}
