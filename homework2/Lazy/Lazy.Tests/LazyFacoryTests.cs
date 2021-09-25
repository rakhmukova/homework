using NUnit.Framework;
using System;

namespace Lazy.Tests
{
    public class LazyFactoryTests
    {
        [Test]
        public void TestInitializeInstancesOfLazyUsingFactory()
        {
            var singleThreadLazy = LazyFactory<int>.CreateSingleThreadLazy(() => 10);
            var multipleThreadsLazy = LazyFactory<double>.CreateMultipleThreadsLazy(() => 8.7 + 5.9);
        }

        [Test]
        public void TestPassNullFunctionToFactoryAndCheckForArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(()
                => LazyFactory<float>.CreateSingleThreadLazy(null));
        }
    }       
}