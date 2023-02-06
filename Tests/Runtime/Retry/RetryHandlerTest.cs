using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Extreal.Core.Logging;
using NUnit.Framework;
using UniRx;
using UnityEngine.TestTools;

namespace Extreal.Core.Common.Retry.Test
{
    public class RetryHandlerTest
    {
        private static readonly ELogger Logger = LoggingManager.GetLogger(nameof(RetryHandlerTest));

        [OneTimeSetUp]
        public void OneTimeSetUp() => LoggingManager.Initialize(logLevel: LogLevel.Debug);

        [Test]
        public void RunAsyncIsNull() =>
            Assert.That(() => RetryHandler<Unit>.Of(null, _ => false, new CountingRetryStrategy()),
                Throws.TypeOf<ArgumentNullException>().With.Message.
                    EqualTo($"Value cannot be null.{Environment.NewLine}Parameter name: runAsync"));

        [Test]
        public void IsRetryableIsNull() =>
            Assert.That(() => RetryHandler<Unit>.Of(() => { }, null, new CountingRetryStrategy()),
                Throws.TypeOf<ArgumentNullException>().With.Message.
                    EqualTo($"Value cannot be null.{Environment.NewLine}Parameter name: isRetryable"));

        [Test]
        public void IsRetryStrategyIsNull() =>
            Assert.That(() => RetryHandler<Unit>.Of(() => { }, _ => false, null),
                Throws.TypeOf<ArgumentNullException>().With.Message.
                    EqualTo($"Value cannot be null.{Environment.NewLine}Parameter name: retryStrategy"));

        [UnityTest]
        public IEnumerator RunActionWithNoRetryStrategyForNoException() => UniTask.ToCoroutine(async () =>
        {
            const string value = "RETURN RETRY TEST";

            var onRetryingValues = new List<int>();
            var onRetriedValue = false;
            var isInvokeOnRetried = false;
            var retryStrategy = new NoRetryStrategy();
            var target = new ClassWithRetry(0);
            using var sut = RetryHandler<Unit>.Of(
                () => target.RunAction(value), e => e is AccessViolationException, retryStrategy);
            using var disposable1 = sut.OnRetrying.Subscribe(onRetryingValues.Add);
            using var disposable2 = sut.OnRetried.Subscribe(result =>
            {
                isInvokeOnRetried = true;
                onRetriedValue = result;
            });

            await sut.HandleAsync();

            Assert.That(target.RunCount, Is.EqualTo(1));
            Assert.That(target.ThrowCount, Is.EqualTo(0));
            Assert.That(onRetryingValues.ToArray(), Is.EqualTo(new int[] { }));
            Assert.That(isInvokeOnRetried, Is.EqualTo(false));
            Assert.That(onRetriedValue, Is.EqualTo(false));
        });

        [UnityTest]
        public IEnumerator RunActionWithNoRetryStrategyForNoRecovery() => UniTask.ToCoroutine(async () =>
        {
            const string value = "RETURN RETRY TEST";

            var onRetryingValues = new List<int>();
            var onRetriedValue = false;
            var isInvokeOnRetried = false;
            var retryStrategy = new NoRetryStrategy();
            var target = new ClassWithRetry(3);
            using var sut = RetryHandler<Unit>.Of(
                () => target.RunAction(value), e => e is AccessViolationException, retryStrategy);
            using var disposable1 = sut.OnRetrying.Subscribe(onRetryingValues.Add);
            using var disposable2 = sut.OnRetried.Subscribe(result =>
            {
                isInvokeOnRetried = true;
                onRetriedValue = result;
            });

            try
            {
                await sut.HandleAsync();
                Assert.Fail("AccessViolationException not happen");
            }
            catch (AccessViolationException e)
            {
                Assert.That(e.Message, Is.EqualTo("THROW RETRY TEST"));
                Assert.That(target.RunCount, Is.EqualTo(1));
                Assert.That(target.ThrowCount, Is.EqualTo(1));
                Assert.That(onRetryingValues.ToArray(), Is.EqualTo(new int[] { }));
                Assert.That(isInvokeOnRetried, Is.EqualTo(false));
                Assert.That(onRetriedValue, Is.EqualTo(false));
            }
        });

        [UnityTest]
        public IEnumerator RunActionWithCountingRetryStrategyForNoException() => UniTask.ToCoroutine(async () =>
        {
            const string value = "RETURN RETRY TEST";

            var onRetryingValues = new List<int>();
            var onRetriedValue = false;
            var isInvokeOnRetried = false;
            var retryStrategy = new CountingRetryStrategy();
            var target = new ClassWithRetry(0);
            using var sut = RetryHandler<Unit>.Of(
                () => target.RunAction(value), e => e is AccessViolationException, retryStrategy);
            using var disposable1 = sut.OnRetrying.Subscribe(onRetryingValues.Add);
            using var disposable2 = sut.OnRetried.Subscribe(result =>
            {
                isInvokeOnRetried = true;
                onRetriedValue = result;
            });

            await sut.HandleAsync();

            Assert.That(target.RunCount, Is.EqualTo(1));
            Assert.That(target.ThrowCount, Is.EqualTo(0));
            Assert.That(onRetryingValues.ToArray(), Is.EqualTo(new int[] { }));
            Assert.That(isInvokeOnRetried, Is.EqualTo(false));
            Assert.That(onRetriedValue, Is.EqualTo(false));
        });

        [UnityTest]
        public IEnumerator RunActionWithCountingRetryStrategyForRecovery() => UniTask.ToCoroutine(async () =>
        {
            const string value = "RETURN RETRY TEST";

            var onRetryingValues = new List<int>();
            var onRetriedValue = false;
            var isInvokeOnRetried = false;
            var retryStrategy = new CountingRetryStrategy(3);
            var target = new ClassWithRetry(3);
            using var sut = RetryHandler<Unit>.Of(
                () => target.RunAction(value), e => e is AccessViolationException, retryStrategy);
            using var disposable1 = sut.OnRetrying.Subscribe(onRetryingValues.Add);
            using var disposable2 = sut.OnRetried.Subscribe(result =>
            {
                isInvokeOnRetried = true;
                onRetriedValue = result;
            });

            await sut.HandleAsync();

            Assert.That(target.RunCount, Is.EqualTo(4));
            Assert.That(target.ThrowCount, Is.EqualTo(3));
            Assert.That(onRetryingValues.ToArray(), Is.EqualTo(new int[] { 1, 2, 3 }));
            Assert.That(isInvokeOnRetried, Is.EqualTo(true));
            Assert.That(onRetriedValue, Is.EqualTo(true));
        });

        [UnityTest]
        public IEnumerator RunActionWithCountingRetryStrategyForNoRecovery() => UniTask.ToCoroutine(async () =>
        {
            const string value = "RETURN RETRY TEST";

            var onRetryingValues = new List<int>();
            var onRetriedValue = false;
            var isInvokeOnRetried = false;
            var retryStrategy = new CountingRetryStrategy(2);
            var target = new ClassWithRetry(3);
            using var sut = RetryHandler<Unit>.Of(
                () => target.RunAction(value), e => e is AccessViolationException, retryStrategy);
            using var disposable1 = sut.OnRetrying.Subscribe(onRetryingValues.Add);
            using var disposable2 = sut.OnRetried.Subscribe(result =>
            {
                isInvokeOnRetried = true;
                onRetriedValue = result;
            });

            try
            {
                await sut.HandleAsync();
                Assert.Fail("AccessViolationException not happen");
            }
            catch (AccessViolationException e)
            {
                Assert.That(e.Message, Is.EqualTo("THROW RETRY TEST"));
                Assert.That(target.RunCount, Is.EqualTo(3));
                Assert.That(target.ThrowCount, Is.EqualTo(3));
                Assert.That(onRetryingValues.ToArray(), Is.EqualTo(new int[] { 1, 2 }));
                Assert.That(isInvokeOnRetried, Is.EqualTo(true));
                Assert.That(onRetriedValue, Is.EqualTo(false));
            }
        });

        [UnityTest]
        public IEnumerator Reuse() => UniTask.ToCoroutine(async () =>
        {
            const string value = "RETURN RETRY TEST";

            var onRetryingValues = new List<int>();
            var onRetriedValue = false;
            var isInvokeOnRetried = false;
            var retryStrategy = new CountingRetryStrategy(2);
            var target = new ClassWithRetry(3);
            using var sut = RetryHandler<Unit>.Of(
                () => target.RunAction(value), e => e is AccessViolationException, retryStrategy);
            using var disposable1 = sut.OnRetrying.Subscribe(onRetryingValues.Add);
            using var disposable2 = sut.OnRetried.Subscribe(result =>
            {
                isInvokeOnRetried = true;
                onRetriedValue = result;
            });

            try
            {
                await sut.HandleAsync();
                Assert.Fail("AccessViolationException not happen");
            }
            catch (AccessViolationException e)
            {
                Assert.That(e.Message, Is.EqualTo("THROW RETRY TEST"));
                Assert.That(target.RunCount, Is.EqualTo(3));
                Assert.That(target.ThrowCount, Is.EqualTo(3));
                Assert.That(onRetryingValues.ToArray(), Is.EqualTo(new int[] { 1, 2 }));
                Assert.That(isInvokeOnRetried, Is.EqualTo(true));
                Assert.That(onRetriedValue, Is.EqualTo(false));
            }

            target.Reset();

            try
            {
                await sut.HandleAsync();
                Assert.Fail("AccessViolationException not happen");
            }
            catch (AccessViolationException e)
            {
                Assert.That(e.Message, Is.EqualTo("THROW RETRY TEST"));
                Assert.That(target.RunCount, Is.EqualTo(3));
                Assert.That(target.ThrowCount, Is.EqualTo(3));
                Assert.That(onRetryingValues.ToArray(), Is.EqualTo(new int[] { 1, 2, 1, 2 }));
                Assert.That(isInvokeOnRetried, Is.EqualTo(true));
                Assert.That(onRetriedValue, Is.EqualTo(false));
            }

            target.Reset();

            try
            {
                await sut.HandleAsync();
                Assert.Fail("AccessViolationException not happen");
            }
            catch (AccessViolationException e)
            {
                Assert.That(e.Message, Is.EqualTo("THROW RETRY TEST"));
                Assert.That(target.RunCount, Is.EqualTo(3));
                Assert.That(target.ThrowCount, Is.EqualTo(3));
                Assert.That(onRetryingValues.ToArray(), Is.EqualTo(new int[] { 1, 2, 1, 2, 1, 2 }));
                Assert.That(isInvokeOnRetried, Is.EqualTo(true));
                Assert.That(onRetriedValue, Is.EqualTo(false));
            }
        });

        [UnityTest]
        public IEnumerator Cancel() => UniTask.ToCoroutine(async () =>
        {
            const string value = "RETURN RETRY TEST";

            using var cts = new CancellationTokenSource();
            var onRetryingValues = new List<int>();
            var isInvokeOnRetried = false;
            var retryStrategy = new CountingRetryStrategy(10);
            var target = new ClassWithRetry(10);
            using var sut = RetryHandler<Unit>.Of(
                () => target.RunAction(value), e => e is AccessViolationException, retryStrategy, cts.Token);
            using var disposable1 = sut.OnRetrying.Subscribe(retryCount =>
            {
                onRetryingValues.Add(retryCount);
                if (retryCount == 4)
                {
                    cts.Cancel();
                }
            });
            using var disposable2 = sut.OnRetried.Subscribe(_ => isInvokeOnRetried = true);

            try
            {
                await sut.HandleAsync();
            }
            catch (OperationCanceledException e)
            {
                Logger.LogDebug("check inner exception", e);
                Assert.That(e.Message, Is.EqualTo("The retry was canceled"));
                Assert.That(target.RunCount, Is.EqualTo(5));
                Assert.That(target.ThrowCount, Is.EqualTo(5));
                Assert.That(onRetryingValues.ToArray(), Is.EqualTo(new int[] { 1, 2, 3, 4 }));
                Assert.That(isInvokeOnRetried, Is.EqualTo(false));
            }
        });

        [UnityTest]
        public IEnumerator RunActionAsyncWithNoRetryStrategyForNoException() => UniTask.ToCoroutine(async () =>
        {
            const string value = "RETURN RETRY TEST";

            var retryStrategy = new NoRetryStrategy();
            var target = new ClassWithRetry(0);
            using var sut = RetryHandler<Unit>.Of(
                async () => await target.RunActionAsync(value), e => e is AccessViolationException, retryStrategy);

            await sut.HandleAsync();

            Assert.That(target.RunCount, Is.EqualTo(1));
            Assert.That(target.ThrowCount, Is.EqualTo(0));
        });

        [UnityTest]
        public IEnumerator RunActionAsyncWithNoRetryStrategyForNoRecovery() => UniTask.ToCoroutine(async () =>
        {
            const string value = "RETURN RETRY TEST";

            var retryStrategy = new NoRetryStrategy();
            var target = new ClassWithRetry(3);
            using var sut = RetryHandler<Unit>.Of(
                async () => await target.RunActionAsync(value), e => e is AccessViolationException, retryStrategy);

            try
            {
                await sut.HandleAsync();
                Assert.Fail("AccessViolationException not happen");
            }
            catch (AccessViolationException e)
            {
                Assert.That(e.Message, Is.EqualTo("THROW RETRY TEST"));
                Assert.That(target.RunCount, Is.EqualTo(1));
                Assert.That(target.ThrowCount, Is.EqualTo(1));
            }
        });

        [UnityTest]
        public IEnumerator RunActionAsyncWithCountingRetryStrategyForNoException() => UniTask.ToCoroutine(async () =>
        {
            const string value = "RETURN RETRY TEST";

            var retryStrategy = new CountingRetryStrategy();
            var target = new ClassWithRetry(0);
            using var sut = RetryHandler<Unit>.Of(
                async () => await target.RunActionAsync(value), e => e is AccessViolationException, retryStrategy);

            await sut.HandleAsync();

            Assert.That(target.RunCount, Is.EqualTo(1));
            Assert.That(target.ThrowCount, Is.EqualTo(0));
        });

        [UnityTest]
        public IEnumerator RunActionAsyncWithCountingRetryStrategyForRecovery() => UniTask.ToCoroutine(async () =>
        {
            const string value = "RETURN RETRY TEST";

            var retryStrategy = new CountingRetryStrategy(3);
            var target = new ClassWithRetry(3);
            using var sut = RetryHandler<Unit>.Of(
                async () => await target.RunActionAsync(value), e => e is AccessViolationException, retryStrategy);

            await sut.HandleAsync();

            Assert.That(target.RunCount, Is.EqualTo(4));
            Assert.That(target.ThrowCount, Is.EqualTo(3));
        });

        [UnityTest]
        public IEnumerator RunActionAsyncWithCountingRetryStrategyForNoRecovery() => UniTask.ToCoroutine(async () =>
        {
            const string value = "RETURN RETRY TEST";

            var retryStrategy = new CountingRetryStrategy(2);
            var target = new ClassWithRetry(3);
            using var sut = RetryHandler<Unit>.Of(
                async () => await target.RunActionAsync(value), e => e is AccessViolationException, retryStrategy);

            try
            {
                await sut.HandleAsync();
                Assert.Fail("AccessViolationException not happen");
            }
            catch (AccessViolationException e)
            {
                Assert.That(e.Message, Is.EqualTo("THROW RETRY TEST"));
                Assert.That(target.RunCount, Is.EqualTo(3));
                Assert.That(target.ThrowCount, Is.EqualTo(3));
            }
        });

        [UnityTest]
        public IEnumerator RunFuncWithNoRetryStrategyForNoException() => UniTask.ToCoroutine(async () =>
        {
            const string value = "RETURN RETRY TEST";

            var retryStrategy = new NoRetryStrategy();
            var target = new ClassWithRetry(0);
            using var sut = RetryHandler<string>.Of(
                () => target.RunFunc(value), e => e is AccessViolationException, retryStrategy);

            var result = await sut.HandleAsync();

            Assert.That(result, Is.EqualTo("RETURN RETRY TEST"));
            Assert.That(target.RunCount, Is.EqualTo(1));
            Assert.That(target.ThrowCount, Is.EqualTo(0));
        });

        [UnityTest]
        public IEnumerator RunFuncWithNoRetryStrategyForNoRecovery() => UniTask.ToCoroutine(async () =>
        {
            const string value = "RETURN RETRY TEST";

            var retryStrategy = new NoRetryStrategy();
            var target = new ClassWithRetry(3);
            using var sut = RetryHandler<string>.Of(
                () => target.RunFunc(value), e => e is AccessViolationException, retryStrategy);

            try
            {
                await sut.HandleAsync();
                Assert.Fail("AccessViolationException not happen");
            }
            catch (AccessViolationException e)
            {
                Assert.That(e.Message, Is.EqualTo("THROW RETRY TEST"));
                Assert.That(target.RunCount, Is.EqualTo(1));
                Assert.That(target.ThrowCount, Is.EqualTo(1));
            }
        });

        [UnityTest]
        public IEnumerator RunFuncWithCountingRetryStrategyForNoException() => UniTask.ToCoroutine(async () =>
        {
            const string value = "RETURN RETRY TEST";

            var retryStrategy = new CountingRetryStrategy();
            var target = new ClassWithRetry(0);
            using var sut = RetryHandler<string>.Of(
                () => target.RunFunc(value), e => e is AccessViolationException, retryStrategy);

            var result = await sut.HandleAsync();

            Assert.That(result, Is.EqualTo("RETURN RETRY TEST"));
            Assert.That(target.RunCount, Is.EqualTo(1));
            Assert.That(target.ThrowCount, Is.EqualTo(0));
        });

        [UnityTest]
        public IEnumerator RunFuncWithCountingRetryStrategyForRecovery() => UniTask.ToCoroutine(async () =>
        {
            const string value = "RETURN RETRY TEST";

            var retryStrategy = new CountingRetryStrategy(3);
            var target = new ClassWithRetry(3);
            using var sut = RetryHandler<string>.Of(
                () => target.RunFunc(value), e => e is AccessViolationException, retryStrategy);

            var result = await sut.HandleAsync();

            Assert.That(result, Is.EqualTo("RETURN RETRY TEST"));
            Assert.That(target.RunCount, Is.EqualTo(4));
            Assert.That(target.ThrowCount, Is.EqualTo(3));
        });

        [UnityTest]
        public IEnumerator RunFuncWithCountingRetryStrategyForNoRecovery() => UniTask.ToCoroutine(async () =>
        {
            const string value = "RETURN RETRY TEST";

            var retryStrategy = new CountingRetryStrategy(2);
            var target = new ClassWithRetry(3);
            using var sut = RetryHandler<string>.Of(
                () => target.RunFunc(value), e => e is AccessViolationException, retryStrategy);

            try
            {
                await sut.HandleAsync();
                Assert.Fail("AccessViolationException not happen");
            }
            catch (AccessViolationException e)
            {
                Assert.That(e.Message, Is.EqualTo("THROW RETRY TEST"));
                Assert.That(target.RunCount, Is.EqualTo(3));
                Assert.That(target.ThrowCount, Is.EqualTo(3));
            }
        });

        [UnityTest]
        public IEnumerator RunFuncAsyncWithNoRetryStrategyForNoException() => UniTask.ToCoroutine(async () =>
        {
            const string value = "RETURN RETRY TEST";

            var retryStrategy = new NoRetryStrategy();
            var target = new ClassWithRetry(0);
            using var sut = RetryHandler<string>.Of(
                async () => await target.RunFuncAsync(value), e => e is AccessViolationException, retryStrategy);

            var result = await sut.HandleAsync();

            Assert.That(result, Is.EqualTo("RETURN RETRY TEST"));
            Assert.That(target.RunCount, Is.EqualTo(1));
            Assert.That(target.ThrowCount, Is.EqualTo(0));
        });

        [UnityTest]
        public IEnumerator RunFuncAsyncWithNoRetryStrategyForNoRecovery() => UniTask.ToCoroutine(async () =>
        {
            const string value = "RETURN RETRY TEST";

            var retryStrategy = new NoRetryStrategy();
            var target = new ClassWithRetry(3);
            using var sut = RetryHandler<string>.Of(
                async () => await target.RunFuncAsync(value), e => e is AccessViolationException, retryStrategy);

            try
            {
                await sut.HandleAsync();
                Assert.Fail("AccessViolationException not happen");
            }
            catch (AccessViolationException e)
            {
                Assert.That(e.Message, Is.EqualTo("THROW RETRY TEST"));
                Assert.That(target.RunCount, Is.EqualTo(1));
                Assert.That(target.ThrowCount, Is.EqualTo(1));
            }
        });

        [UnityTest]
        public IEnumerator RunFuncAsyncWithCountingRetryStrategyForNoException() => UniTask.ToCoroutine(async () =>
        {
            const string value = "RETURN RETRY TEST";

            var retryStrategy = new CountingRetryStrategy();
            var target = new ClassWithRetry(0);
            using var sut = RetryHandler<string>.Of(
                async () => await target.RunFuncAsync(value), e => e is AccessViolationException, retryStrategy);

            var result = await sut.HandleAsync();

            Assert.That(result, Is.EqualTo("RETURN RETRY TEST"));
            Assert.That(target.RunCount, Is.EqualTo(1));
            Assert.That(target.ThrowCount, Is.EqualTo(0));
        });

        [UnityTest]
        public IEnumerator RunFuncAsyncWithCountingRetryStrategyForRecovery() => UniTask.ToCoroutine(async () =>
        {
            const string value = "RETURN RETRY TEST";

            var retryStrategy = new CountingRetryStrategy(3);
            var target = new ClassWithRetry(3);
            using var sut = RetryHandler<string>.Of(
                async () => await target.RunFuncAsync(value), e => e is AccessViolationException, retryStrategy);

            var result = await sut.HandleAsync();

            Assert.That(result, Is.EqualTo("RETURN RETRY TEST"));
            Assert.That(target.RunCount, Is.EqualTo(4));
            Assert.That(target.ThrowCount, Is.EqualTo(3));
        });

        [UnityTest]
        public IEnumerator RunFuncAsyncWithCountingRetryStrategyForNoRecovery() => UniTask.ToCoroutine(async () =>
        {
            const string value = "RETURN RETRY TEST";

            var retryStrategy = new CountingRetryStrategy(2);
            var target = new ClassWithRetry(3);
            using var sut = RetryHandler<string>.Of(
                async () => await target.RunFuncAsync(value), e => e is AccessViolationException, retryStrategy);

            try
            {
                await sut.HandleAsync();
                Assert.Fail("AccessViolationException not happen");
            }
            catch (AccessViolationException e)
            {
                Assert.That(e.Message, Is.EqualTo("THROW RETRY TEST"));
                Assert.That(target.RunCount, Is.EqualTo(3));
                Assert.That(target.ThrowCount, Is.EqualTo(3));
            }
        });

        [UnityTest]
        public IEnumerator ReuseWithParameter() => UniTask.ToCoroutine(async () =>
        {
            const string value = "RETURN RETRY TEST";

            var retryStrategy = new CountingRetryStrategy(3);
            var target = new ClassWithRetry(3);
            using var sut = RetryHandler<string>.Of(
                async () => await target.RunFuncAsync(value), e => e is AccessViolationException, retryStrategy);

            var result = await sut.HandleAsync();

            Assert.That(result, Is.EqualTo("RETURN RETRY TEST"));
            Assert.That(target.RunCount, Is.EqualTo(4));
            Assert.That(target.ThrowCount, Is.EqualTo(3));

            target.Reset();

            result = await sut.HandleAsync();

            Assert.That(result, Is.EqualTo("RETURN RETRY TEST"));
            Assert.That(target.RunCount, Is.EqualTo(4));
            Assert.That(target.ThrowCount, Is.EqualTo(3));

            target.Reset();

            result = await sut.HandleAsync();

            Assert.That(result, Is.EqualTo("RETURN RETRY TEST"));
            Assert.That(target.RunCount, Is.EqualTo(4));
            Assert.That(target.ThrowCount, Is.EqualTo(3));
        });
    }
}
