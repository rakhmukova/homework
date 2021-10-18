using System;

namespace MyThreadPool
{
    /// <summary>
    /// An interface that represents a single opertion which returns a value.
    /// </summary>
    /// <typeparam name="TResult">A type of returned value.</typeparam>
    public interface IMyTask<out TResult>
    {
        /// <summary>
        /// Gets a value indicating whether the result has been calculated.
        /// </summary>
        public bool IsCompleted { get; }

        /// <summary>
        /// Gets a result of completed operation.
        /// </summary>
        public TResult Result { get; }

        /// <summary>
        /// Returns a task that applies an operation to the result of a current task.
        /// </summary>
        /// <typeparam name="TNewResult">A type of a value returned by created task.</typeparam>
        /// <param name="function">An operation that should be completed.</param>
        public IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> function);
    }
}
