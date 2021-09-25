using NUnit.Framework;
using System;

namespace Lazy.Tests
{
    public class SingleThreadLazyTests
    {
        [Test]
        public void TestInitializeStringLazyAndCompareValues()
        {
            var callsNum = 1000;
            var lazy = LazyFactory<string>.CreateSingleThreadLazy(() => "go" + " " + "nuts");
            for (var i = 0; i < callsNum; ++i)
            {
                Assert.AreEqual("go nuts", lazy.Get());
            }
        }

        [Test]
        public void TestExceptionInsideSupplierFunc()
        {
            var callsNum = 10;
            var lazy = LazyFactory<float>.CreateSingleThreadLazy(() 
                => throw new NotImplementedException());
            for (var i = 0; i < callsNum; ++i)
            {
                Assert.Throws<NotImplementedException>(() => lazy.Get());
            }
        }

        [Test]
        public void TestInitializeCharLazyAndCheckCalculationsAreDoneOnce()
        {
            var callsNum = 1000;
            var called = 0;
            var lazy = LazyFactory<char>.CreateSingleThreadLazy(() =>
            {
                if (called > 0)
                {
                    return 'b';
                }
                ++called;
                return 'a';
            });
            
            for (var i = 0; i < callsNum; ++i)
            {
                Assert.AreEqual('a', lazy.Get());
            }
            Assert.AreEqual(1, called);
        }
    }
}
