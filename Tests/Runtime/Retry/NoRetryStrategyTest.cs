using System;
using NUnit.Framework;

namespace Extreal.Core.Common.Retry.Test
{
    public class NoRetryStrategyTest
    {
        [Test]
        public void InvalidUsage()
        {
            var sut = new NoRetryStrategy();
            Assert.That(() => sut.Next(),
                Throws.TypeOf<InvalidOperationException>().With.Message.EqualTo("Unreachable"));
        }
    }
}
