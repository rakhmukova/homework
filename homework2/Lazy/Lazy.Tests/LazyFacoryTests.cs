using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Lazy.Tests
{
    public class LazyFactoryTests
    {
        private static IEnumerable<ILazy<int>> LazyInstances()
        {
            int singleThreadedCount = 0;
            yield return LazyFactory.CreateSingleThreadLazy(()
                =>
            {
                ++singleThreadedCount;
                return singleThreadedCount;
            });

            int multiThreadedCount = 0;
            yield return LazyFactory.CreateMultipleThreadsLazy(()
                =>
            {
                Interlocked.Increment(ref multiThreadedCount);
                return multiThreadedCount;
            });
        }

        [TestCaseSource(nameof(LazyInstances))]
        public void TestCheckInitializedLazyInstancesAreNotNull<T>(ILazy<T> lazy)
        {
            Assert.NotNull(lazy);
        }

        [TestCaseSource(nameof(LazyInstances))]
        public void TestInitializeLazyAndCheckCalculationsAreDoneOnce(ILazy<int> lazy)
        {
            int callsNum = 100;
            for (var i = 0; i < callsNum; ++i)
            {
                Assert.AreEqual(1, lazy.Get());
            }
        }

        [Test]
        public void TestPassNullFunctionToFactoryAndCheckForArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(()
                => LazyFactory.CreateSingleThreadLazy<float>(null));
            Assert.Throws<ArgumentNullException>(()
                => LazyFactory.CreateMultipleThreadsLazy<float>(null));
        }
    }       
}