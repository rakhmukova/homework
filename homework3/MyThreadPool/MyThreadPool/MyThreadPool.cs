using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace MyThreadPool
{
    /// <summary>
    /// A class that provides a pool of threads to operate tasks.
    /// </summary>
    public class MyThreadPool
    {
        private readonly Thread[] threads;
        private readonly BlockingCollection<Action> waitingTasks = new ();
        private readonly object lockObject = new ();
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly CancellationToken token;

        /// <summary>
        /// Initializes a new instance of the <see cref="MyThreadPool"/> class.
        /// </summary>
        /// <param name="threadsNum">A preferrable number of threads.</param>
        public MyThreadPool(int threadsNum)
        {
            if (threadsNum <= 0)
            {
                throw new ArgumentException(
                    "The number of threads should be positive",
                    nameof(threadsNum));
            }

            this.cancellationTokenSource = new ();
            this.token = cancellationTokenSource.Token;
            this.threads = new Thread[threadsNum];
            for (var i = 0; i < threadsNum; ++i)
            {
                threads[i] = new (ProcessTasks);
                threads[i].IsBackground = true;
                threads[i].Start();
            }
        }

        /// <summary>
        /// Gets the number of threads in the pool.
        /// </summary>
        public int ThreadsNum => threads.Length;

        private void ProcessTasks()
        {
            while (true)
            {
                try
                {
                    var action = waitingTasks.Take(token);
                    action.Invoke();
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Add an action to the queue of operations returns true,
        /// if it is impossible, returns false.
        /// </summary>
        /// <param name="action">An operation to add.</param>
        public bool TryEnqueueAction(Action action)
        {
            lock (lockObject)
            {
                if (token.IsCancellationRequested)
                {
                    return false;
                }

                waitingTasks.Add(action);
                return true;
            }
        }

        /// <summary>
        /// Returns an instance of MyTask class.
        /// </summary>
        /// <param name="function">An operation to complete.</param>
        public IMyTask<TResult> Submit<TResult>(Func<TResult> function)
        {
            if (!token.IsCancellationRequested)
            {
                var myTask = new MyTask<TResult>(function, this, this.token);
                if (TryEnqueueAction(() => myTask.Execute()))
                {
                    return myTask;
                }
            }

            throw new InvalidOperationException("ThreadPool stopped working.");
        }

        /// <summary>
        /// Stops a work of threads in the pool.
        /// </summary>
        public void ShutDown()
        {
            lock (lockObject)
            {
                cancellationTokenSource.Cancel();
                waitingTasks.CompleteAdding();
            }

            while (threads.Any(thread => thread.IsAlive));
        }
    }
}