using System;
using NUnit.Framework;

namespace Extreal.Core.Common.Retry.Test
{
    public class CountingRetryStrategyTest
    {
        [Test]
        public void MaxRetryCountIsLessThan1()
            => Assert.That(() => new CountingRetryStrategy(0),
                Throws.TypeOf<ArgumentOutOfRangeException>().With.Message.EqualTo(
                    "Please specify 1 or more" + Environment.NewLine +
                    "Parameter name: maxRetryCount" + Environment.NewLine +
                    "Actual value was 0."));

        [Test]
        public void MaxRetryCountIsTooBig() =>
            Assert.That(() => new CountingRetryStrategy(21),
                Throws.TypeOf<ArgumentOutOfRangeException>().With.Message.EqualTo(
                    "The default for nextRetryInterval is to use 20 Fibonacci numbers, so maxRetryCount must be less than or equal to 20. Alternatively, specify nextRetryInterval" +
                    Environment.NewLine +
                    "Parameter name: maxRetryCount" +
                    Environment.NewLine +
                    "Actual value was 21."));
    }
}
