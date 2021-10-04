using NUnit.Framework;
using System;

namespace Lazy.Tests
{
    public class LazyFactoryTests
    {
        [Test]
        public void TestInitializeInstancesOfLazyUsingFactory()
        {
            var singleThreadLazy = LazyFactory.CreateSingleThreadLazy(() => 10);
            var multipleThreadsLazy = LazyFactory.CreateMultipleThreadsLazy(() => 8.7 + 5.9);
        }

        [Test]
        public void TestPassNullFunctionToFactoryAndCheckForArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(()
                => LazyFactory.CreateSingleThreadLazy<float>(null));
        }
    }       
}