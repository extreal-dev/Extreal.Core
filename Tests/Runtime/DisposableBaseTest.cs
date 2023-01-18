using System;
using NUnit.Framework;
using UnityEngine;

namespace Extreal.Core.System.Test
{
    public class DisposableBaseTest
    {
        [Test]
        public void DisposeSuccess()
        {
            var testClass = new DisposableSubClass();
            Assert.AreNotEqual(0, testClass.List.Count);
            Assert.AreEqual(0, testClass.Stream.Length);

            testClass.Dispose();

            Assert.AreEqual(0, testClass.List.Count);
            Assert.That(() => _ = testClass.Stream.Length,
                Throws.TypeOf<ObjectDisposedException>());
        }

        [Test]
        public void DisposeTwice()
        {
            var receivedLogText = string.Empty;
            void LogMessageReceivedHandler(string condition, string stackTrace, LogType type)
                => receivedLogText = condition;
            Application.logMessageReceived += LogMessageReceivedHandler;

            var testClass = new DisposableSubClass();
            testClass.Dispose();
            testClass.Dispose();
            Assert.IsEmpty(receivedLogText);

            Application.logMessageReceived -= LogMessageReceivedHandler;
        }

        [Test]
        public void DisposableWithoutOverrideVirtualMethods()
        {
            var receivedLogText = string.Empty;
            void LogMessageReceivedHandler(string condition, string stackTrace, LogType type)
                => receivedLogText = condition;
            Application.logMessageReceived += LogMessageReceivedHandler;

            var emptyTestClass = new DisposableEmptyClass();
            emptyTestClass.Dispose();

            Application.logMessageReceived -= LogMessageReceivedHandler;
        }
    }
}
