using System;
using System.Collections.Concurrent;
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

        private bool TryEnqueueAction(Action action)
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
                if (TryEnqueueAction(() => myTask.Run()))
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

            foreach (var thread in threads)
            {
                thread.Join();
            }
        }

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
        /// A class that represents a single operation which returns a value.
        /// </summary>
        /// <typeparam name="TResult">A type of returned value.</typeparam>
        private class MyTask<TResult> : IMyTask<TResult>
        {
            private readonly object lockObject = new ();
            private readonly ManualResetEvent isResultAvailable = new (false);
            private readonly ManualResetEvent calculationStarted = new (false);
            private readonly MyThreadPool threadPool;
            private readonly CancellationToken token;
            private readonly ConcurrentQueue<Action> tasksToContinueWith = new ();
            private TResult result;
            private Func<TResult> function;
            private AggregateException exception;

            /// <summary>
            /// Initializes a new instance of the <see cref="MyTask{TResult}"/> class.
            /// </summary>
            /// <param name="function">An operation to be completed.</param>
            /// <param name="threadPool">A ThreadPool object,
            /// whose thread is to complete an operation.</param>
            /// <param name="token">A threadpool token.</param>
            public MyTask(Func<TResult> function, MyThreadPool threadPool, CancellationToken token)
            {
                this.function = function
                    ?? throw new ArgumentNullException(nameof(function));
                this.threadPool = threadPool
                    ?? throw new ArgumentNullException(nameof(threadPool));
                this.token = token;
            }

            /// <summary>
            /// Gets a value indicating whether the result has been calculated.
            /// </summary>
            public bool IsCompleted { get; private set; }

            /// <summary>
            /// Gets a result of completed operation.
            /// </summary>
            public TResult Result
            {
                get
                {
                    if (!IsCompleted)
                    {
                        var events = new WaitHandle[]
                        {
                            isResultAvailable,
                            calculationStarted,
                            token.WaitHandle,
                        };

                        var eventIndex = WaitHandle.WaitAny(events);
                        if (events[eventIndex] == token.WaitHandle)
                        {
                            var innerException = new InvalidOperationException("ThreadPool stopped working.");
                            this.exception = new (innerException);
                        }
                        else
                        {
                            isResultAvailable.WaitOne();
                        }
                    }

                    if (exception != null)
                    {
                        throw exception;
                    }

                    return result;
                }
            }

            /// <summary>
            /// Returns a task that applies an operation to the result of a current task.
            /// </summary>
            /// <typeparam name="TNewResult">A type of a value returned by created task.</typeparam>
            /// <param name="function">An operation that should be completed.</param>
            public IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> function)
            {
                var eventualFunction = new Func<TNewResult>(() => function(this.Result));
                var newTask = new MyTask<TNewResult>(eventualFunction, this.threadPool, this.token);
                var action = new Action(() => newTask.Run());
                lock (lockObject)
                {
                    if (IsCompleted)
                    {
                        threadPool.TryEnqueueAction(action);
                    }
                    else
                    {
                        tasksToContinueWith.Enqueue(action);
                    }
                }

                return newTask;
            }

            /// <summary>
            /// Calculates the value of opearation.
            /// </summary>
            public void Run()
            {
                calculationStarted.Set();

                try
                {
                    result = function();
                }
                catch (Exception innerException)
                {
                    exception = new AggregateException(innerException);
                }

                lock (lockObject)
                {
                    function = null;
                    IsCompleted = true;
                    isResultAvailable.Set();
                    while (tasksToContinueWith.TryDequeue(out Action action))
                    {
                        if (!threadPool.TryEnqueueAction(action))
                        {
                            tasksToContinueWith.Clear();
                        }
                    }
                }
            }
        }
    }
}