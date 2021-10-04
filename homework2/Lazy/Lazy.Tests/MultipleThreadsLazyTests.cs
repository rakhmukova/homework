using NUnit.Framework;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace Lazy.Tests
{
    public class MultipleThreadsLazyTests
    {
        private static readonly int threadsNum = Environment.ProcessorCount;

        [Test]
        public void TestIntLazyInitializationOccursOnce()
        {            
            var called = 0;
            var lazy = LazyFactory.CreateMultipleThreadsLazy(() =>
            {
                Interlocked.Increment(ref called);
                return called;
            });

            var threads = new Thread[threadsNum];
            var lazyValues = new int[threadsNum];
            for (var i = 0; i < threadsNum; ++i)
            {
                var localI = i;
                threads[i] = new Thread(() => lazyValues[localI] = lazy.Get());
            }

            foreach (var thread in threads)
            {
                thread.Start();
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }

            Assert.AreEqual(1, called);
            CollectionAssert.AreEqual(Enumerable.Repeat(1, threadsNum).ToArray(), lazyValues);
        }

        [Test]
        public void TestConcurrentStackLazyInitializationOccursOnce()
        {
            var stack = new ConcurrentStack<int>();            
            var called = 0;
            var lazy = LazyFactory.CreateMultipleThreadsLazy(() =>
            {
                stack.Push(called);
                Interlocked.Increment(ref called);
                return stack;
            });

            var threads = new Thread[threadsNum];
            var lazyValues = new ConcurrentStack<int>[threadsNum];
            for (var i = 0; i < threadsNum; ++i)
            {
                var localI = i;
                threads[i] = new Thread(() => lazyValues[localI] = lazy.Get());
            }

            foreach (var thread in threads)
            {
                thread.Start();
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }

            var expected = lazyValues[0];
            foreach (var value in lazyValues)
            {
                CollectionAssert.AreEqual(expected, value);
            }
        }

        [Test]
        public void TestLazyLazyInitializationOccursOnce()
        {            
            var called = 0;
            var random = new Random();
            var lazy = LazyFactory.CreateMultipleThreadsLazy(() =>
            {
                Interlocked.Increment(ref called);
                return LazyFactory.CreateMultipleThreadsLazy(() =>
                {
                    Interlocked.Increment(ref called);
                    return random.Next();
                });
            });

            var threads = new Thread[threadsNum];
            var lazyValues = new int[threadsNum];
            for (var i = 0; i < threadsNum; ++i)
            {
                var localI = i;
                threads[i] = new Thread(() => lazyValues[localI] = lazy.Get().Get());
            }

            foreach (var thread in threads)
            {
                thread.Start();
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }

            Assert.AreEqual(2, called);
            var expected = lazyValues[0];
            CollectionAssert.AreEqual(Enumerable.Repeat(expected, threadsNum).ToArray(), lazyValues);
        }
    }
}
