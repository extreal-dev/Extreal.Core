using System;
using NUnit.Framework;

namespace Extreal.Core.Common.Retry.Test
{
    public class NullRetryStrategyTest
    {
        [Test]
        public void InvalidUsage()
        {
            var sut = new NullRetryStrategy();
            Assert.That(() => sut.Next(),
                Throws.TypeOf<InvalidOperationException>().With.Message.EqualTo("Unreachable"));
        }
    }
}
