using System;

namespace Lazy
{
    /// <summary>
    /// A class for creating instances of the classes implementing ILazy<T> interface
    /// </summary>
    public static class LazyFactory
    {
        /// <summary>
        /// Creates an instance of the SingleThreadLazy<T> class
        /// </summary>
        public static SingleThreadLazy<T> CreateSingleThreadLazy<T>(Func<T> supplier)
            => new(supplier);        

        /// <summary>
        /// Creates an instance of the MultipleThreadsLazy<T> class
        /// </summary>
        public static MultipleThreadsLazy<T> CreateMultipleThreadsLazy<T>(Func<T> supplier)
            => new(supplier);
    }
}
