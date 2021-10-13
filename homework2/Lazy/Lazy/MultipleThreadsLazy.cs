using System;
using System.Threading;

namespace Lazy
{
    /// <summary>
    /// A class for lazy initialization in a multi-threaded mode
    /// </summary>
    public class MultipleThreadsLazy<T> : ILazy<T>
    {
        private Func<T> supplier;
        private T value;
        private bool isValueCreated;
        private readonly Object lockObject = new();

        /// <summary>
        /// Initializes an instance of MultipleThreadsLazy<T> class
        /// </summary>
        /// <param name="supplier">An initialization function that is used 
        /// when a lazy initialization occurs</param>
        internal MultipleThreadsLazy(Func<T> supplier)
            => this.supplier = supplier
            ?? throw new ArgumentNullException(nameof(supplier));

        /// <summary>
        /// Returns the lazily initialized value of the current MultipleThreadsLazy<T> instance
        /// </summary>
        public T Get()
        {
            if (!isValueCreated)
            {
                lock (lockObject)
                {
                    if (!Volatile.Read(ref isValueCreated))
                    {
                        value = supplier();
                        Volatile.Write(ref isValueCreated, true);
                        supplier = null;
                    }
                }
            }
            return value;
        }
    }
}
