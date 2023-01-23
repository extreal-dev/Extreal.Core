using System.Collections;
using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Extreal.Core.Test
{
    public class DisposableBaseTest
    {
        private string receivedLogText;

        [SetUp]
        public void Initialize()
            => receivedLogText = string.Empty;

        [UnityTest]
        public IEnumerator DisposeWhenGC()
        {
            _ = new DisposableSubClass();
            yield return null;

            Application.logMessageReceivedThreaded += LogMessageReceivedHandler;

            GC.Collect();
            while (receivedLogText == string.Empty)
            {
                yield return null;
            }
            Assert.AreEqual("Free Resources", receivedLogText);

            Application.logMessageReceivedThreaded -= LogMessageReceivedHandler;
        }

        [Test]
        public void DisposeSuccess()
        {
            var testClass = new DisposableSubClass();
            Assert.AreEqual(0, testClass.Stream.Length);
            Assert.IsFalse(testClass.Uwr.isDone);

            testClass.Dispose();

            Assert.That(() => _ = testClass.Uwr.isDone,
                Throws.TypeOf<ArgumentNullException>());
            Assert.That(() => _ = testClass.Stream.Length,
                Throws.TypeOf<ObjectDisposedException>());
        }

        [Test]
        public void DisposeTwice()
        {
            var testClass = new DisposableSubClass();
            testClass.Dispose();

            Application.logMessageReceived += LogMessageReceivedHandler;

            testClass.Dispose();
            Assert.IsEmpty(receivedLogText);

            Application.logMessageReceived -= LogMessageReceivedHandler;
        }

        [Test]
        public void DisposeWithUsingStatement()
        {
            {
                using var testClass = new DisposableSubClass();
                Assert.AreEqual(0, testClass.Stream.Length);
                Assert.IsFalse(testClass.Uwr.isDone);
            }

            LogAssert.Expect(LogType.Log, "Free Resources");
        }

        private void LogMessageReceivedHandler(string condition, string stackTrace, LogType type)
                => receivedLogText = condition;
    }
}
