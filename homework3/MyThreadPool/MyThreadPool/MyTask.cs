using System;
using System.Collections.Concurrent;
using System.Threading;

namespace MyThreadPool
{
    /// <summary>
    /// A class that represents a single opertion which returns a value.
    /// </summary>
    /// <typeparam name="TResult">A type of returned value.</typeparam>
    public class MyTask<TResult> : IMyTask<TResult>
    {
        private readonly object lockObject = new ();
        private readonly ManualResetEvent resultIsAvailable = new (false);
        private readonly MyThreadPool threadPool;
        private readonly CancellationToken token;
        private readonly ConcurrentQueue<Action> tasksToContinueWith = new ();
        private TResult result;
        private Func<TResult> function;

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
        /// Gets the AggregateException that caused MyTask to end prematurely.
        /// </summary>
        public AggregateException Exception { get; private set; }

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
                    var events = new WaitHandle[] { resultIsAvailable, token.WaitHandle };
                    var eventIndex = WaitHandle.WaitAny(events);
                    if (events[eventIndex] == token.WaitHandle)
                    {
                        var innerException = new InvalidOperationException("ThreadPool stopped working.");
                        this.Exception = new (innerException);
                    }
                }

                if (Exception != null)
                {
                    throw Exception;
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
            var action = new Action(() => newTask.Execute());
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
        public void Execute()
        {
            lock (lockObject)
            {
                try
                {
                    result = function();
                }
                catch (Exception innerException)
                {
                    Exception = new AggregateException(innerException);
                }
                finally
                {
                    function = null;
                    IsCompleted = true;
                    resultIsAvailable.Set();
                    while (tasksToContinueWith.TryDequeue(out Action action))
                    {
                        if (!threadPool.TryEnqueueAction(action))
                        {
                            tasksToContinueWith.Clear();
                            break;
                        }
                    }
                }
            }
        }
    }
}
