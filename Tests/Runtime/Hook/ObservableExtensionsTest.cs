using System;
using System.Text.RegularExpressions;
using Extreal.Core.Logging;
using NUnit.Framework;
using UniRx;
using UnityEngine;
using UnityEngine.TestTools;

namespace Extreal.Core.Common.Hook.Test
{
    public class ObservableExtensionsTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp() => LoggingManager.Initialize(logLevel: LogLevel.Debug);

        private CompositeDisposable disposables;

        [SetUp]
        public void SetUp() => disposables = new CompositeDisposable();

        [TearDown]
        public void TearDown() => disposables.Dispose();

        [Test]
        public void HookIsNull() =>
            Assert.That(() =>
                {
                    var point = new ReactiveProperty<int>(0).AddTo(disposables);
                    point.Hook(null);
                },
                Throws.TypeOf<ArgumentNullException>().With.Message.Contain("Parameter name: hook"));

        [Test]
        public void Hook()
        {
            var total = 0;

            var point = new ReactiveProperty<int>(0).AddTo(disposables);
            Action<int> hook = value => total += value;
            point.Hook(hook).AddTo(disposables);

            point.Value = 1;
            point.Value = 2;
            point.Value = 3;

            Assert.That(total, Is.EqualTo(6));
        }

        [Test]
        public void ExceptionOccurredOnHook()
        {
            LogAssert.ignoreFailingMessages = true;
            LogAssert.Expect(LogType.Error, new Regex(".*Error has occurred on hook.*"));

            var total = 0;

            var point = new ReactiveProperty<int>(0).AddTo(disposables);
            Action<int> onNext = value => total += value;
            Action<int> hook = _ => throw new Exception("HOOK ERROR");
            point.Subscribe(onNext).AddTo(disposables);
            point.Hook(hook).AddTo(disposables);

            point.Value = 1;
            point.Value = 2;
            point.Value = 3;

            Assert.That(total, Is.EqualTo(6));
        }
    }
}
