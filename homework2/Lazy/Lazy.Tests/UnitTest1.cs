using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;

namespace Lazy.Tests
{
    public class LazyFactoryTests
    {
        [SetUp]
        public void Setup()
        {
            var lazy = LazyFactory<int>.CreateSingleThreadLazy(() => 10);
            Assert.AreEqual(10, lazy.Get());
        }

        [Test]
        public void TestNullFuncIsPassedToFactory()
        {
            Assert.Throws<ArgumentNullException>(()
                => LazyFactory<float>.CreateSingleThreadLazy(null));
        }
    }

    public class SingleThreadLazyTests
    {
        [Test]
        public void Test()
        {

        }
    }

    public class MultipleThreadsLazyTests
    {
       
        [Test]
        public void TestCalculationIsExecutedOnce()
        {
            var threadsNum = 8;
            var threads = new Thread[threadsNum];
            var values = new int[threadsNum];
            var callsNum = 0;
            var lazy = LazyFactory<int>.CreateMultipleThreadsLazy(() =>
            {
                ++callsNum;
                return callsNum;
            });

            for (var i = 0; i < threadsNum; ++i)
            {
                var localI = i;
                threads[i] = new Thread(() => values[localI] = lazy.Get());
            }

            foreach (var thread in threads)
            {
                thread.Start();
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }

            CollectionAssert.AreEqual(Enumerable.Repeat(1, threadsNum).ToArray(), values);
        }
    }
}