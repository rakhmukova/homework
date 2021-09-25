using System;

namespace Lazy
{
    /// <summary>
    /// A class for creating instances of the classes implementing ILazy<T> interface
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class LazyFactory<T>
    {
        /// <summary>
        /// Creates an instance of the SingleThreadLazy<T> class
        /// </summary>
        public static SingleThreadLazy<T> CreateSingleThreadLazy(Func<T> supplier)
        {
            return new SingleThreadLazy<T>(supplier);
        }

        /// <summary>
        /// Creates an instance of the MultipleThreadsLazy<T> class
        /// </summary>
        public static MultipleThreadsLazy<T> CreateMultipleThreadsLazy(Func<T> supplier)
        {
            return new MultipleThreadsLazy<T>(supplier);
        }
    }
}
