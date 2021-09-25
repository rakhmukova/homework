using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Lazy.Tests
{
    public class MultipleThreadsLazyTests
    {
        [Test]
        public void TestIntLazyInitializationOccursOnce()
        {
            var threadsNum = Environment.ProcessorCount;
            var threads = new Thread[threadsNum];
            var lazyValues = new int[threadsNum];
            var called = 0;
            var lazy = LazyFactory<int>.CreateMultipleThreadsLazy(() =>
            {
                ++called;
                return called;
            });

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
        public void TestStackLazyInitializationOccursOnce()
        {
            var stack = new Stack<int>();
            var threadsNum = Environment.ProcessorCount;
            var threads = new Thread[threadsNum];
            var lazyValues = new Stack<int>[threadsNum];
            var called = 0;

            var lazy = LazyFactory<Stack<int>>.CreateMultipleThreadsLazy(() =>
            {
                if (called % 2 == 0)
                {
                    stack.Push(called);
                }
                else
                {
                    stack.Pop();
                }
                return stack;
            });

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

            var expected = new Stack<int>();
            expected.Push(0);
            foreach(var value in lazyValues)
            {
                CollectionAssert.AreEqual(expected, value);
            }
        }

        [Test]
        public void TestLazyLazyInitializationOccursOnce()
        {
            var threadsNum = Environment.ProcessorCount;
            var threads = new Thread[threadsNum];
            var lazyValues = new int[threadsNum];
            var called = 0;
            var random = new Random();

            var lazy = LazyFactory<ILazy<int>>.CreateMultipleThreadsLazy(() =>
            {
                ++called;
                return LazyFactory<int>.CreateMultipleThreadsLazy(() =>
                {
                    ++called;
                    return random.Next();
                });
            });

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
