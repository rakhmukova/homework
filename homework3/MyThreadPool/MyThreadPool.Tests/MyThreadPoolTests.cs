using System;
using System.Linq;
using System.Threading;
using NUnit.Framework;

namespace MyThreadPool.Tests
{
    public class MyThreadPoolTests
    {
        private readonly int threadsNum = Environment.ProcessorCount;

        [Test]
        public void TestCheckAmountOfThreadsInPoolIsNotLessThanExpected()
        {
            var expectedThreadsNum = this.threadsNum;
            var pool = new MyThreadPool(expectedThreadsNum);
            var tasks = new IMyTask<int>[expectedThreadsNum];
            var actualThreadsNum = 0;
            var expectedNumberIsReached = new ManualResetEvent(false);
            for (var i = 0; i < expectedThreadsNum; ++i)
            {
                tasks[i] = pool.Submit(() =>
                {
                    Interlocked.Increment(ref actualThreadsNum);
                    if (actualThreadsNum == expectedThreadsNum)
                    {
                        expectedNumberIsReached.Set();
                    }

                    expectedNumberIsReached.WaitOne();
                    return actualThreadsNum;
                });
            }

            foreach (var task in tasks)
            {
                _ = task.Result;
            }

            Assert.AreEqual(expectedThreadsNum, actualThreadsNum);
        }

        [Test]
        public void TestCreateChainOfContinueWithAndCheckResult()
        {
            var pool = new MyThreadPool(this.threadsNum);
            var task = pool.Submit(() => 4 + 66).ContinueWith(a => a * a).
                ContinueWith(b => b.ToString());
            Assert.AreEqual("4900", task.Result);
        }

        [Test]
        public void TestResultIsCalculatedOnce()
        {
            var counter = 0;
            var pool = new MyThreadPool(this.threadsNum);
            var task = pool.Submit(() =>
            {
                Interlocked.Increment(ref counter);
                return counter;
            });
            var num = 100;
            var threads = new Thread[num];
            var results = new int[num];
            for (var i = 0; i < num; ++i)
            {
                var localI = i;
                threads[i] = new (() => results[localI] = task.Result);
            }

            foreach (var thread in threads)
            {
                thread.Start();
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }

            CollectionAssert.AreEqual(
                Enumerable.Repeat(1, num).ToArray(),
                results);
        }

        [Test]
        public void TestCallContinueWithMultipleTimes()
        {
            var pool = new MyThreadPool(this.threadsNum);
            var firstTask = pool.Submit(() => 2);
            var results = Enumerable.Range(1, 10).
                Select((el) => firstTask.ContinueWith(a => a * el).Result);
            CollectionAssert.AreEqual(
                Enumerable.Range(1, 10).Select(a => 2 * a),
                results);
        }

        [Test]
        public void TestTryRunTaskAfterShutDownAndCheckIfExceptionIsThrown()
        {
            var pool = new MyThreadPool(this.threadsNum);
            var firstTask = pool.Submit(() => 0);
            var thread = new Thread(() =>
            {
                pool.ShutDown();
            });
            thread.Start();
            thread.Join();
            var nextTask = firstTask.ContinueWith((value) => -value);

            try
            {
               _ = nextTask.Result;
               Assert.Fail("Exception should be thrown.");
            }
            catch (AggregateException exception)
            {
                var innerExceptions = exception.InnerExceptions;
                Assert.AreEqual(1, innerExceptions.Count);
                Assert.AreEqual(
                    typeof(InvalidOperationException),
                    innerExceptions[0].GetType());
                Assert.AreEqual(
                    "ThreadPool stopped working.",
                    innerExceptions[0].Message);
            }
            catch (Exception)
            {
                Assert.Fail($"The type of exception should be {nameof(AggregateException)}");
            }
        }
    }
}