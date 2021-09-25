using System;

namespace Lazy
{
    /// <summary>
    /// A class for lazy initialization in a single-threaded mode
    /// </summary>
    public class SingleThreadLazy<T> : ILazy<T>
    {
        private Func<T> supplier;
        private T value;
        private bool isValueCreated;

        /// <summary>
        /// Initializes an instance of the SingleThreadLazy<T> class
        /// </summary>
        /// <param name="supplier">An initialization function that is used 
        /// when a lazy initialization occurs</param>
        internal SingleThreadLazy(Func<T> supplier)
        {
            if (supplier == null)
            {
                throw new ArgumentNullException();
            }
            this.supplier = supplier;
        }

        /// <summary>
        /// Returns the lazily initialized value of the current SingleThreadLazy<T> instance
        /// </summary>
        public T Get()
        {
            if (!isValueCreated)
            {
                value = supplier();
                isValueCreated = true;
            }
            return value;
        }
    }
}

