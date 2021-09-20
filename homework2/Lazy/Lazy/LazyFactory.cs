using System;

namespace Lazy
{
    /// <summary>
    /// a class for creating instances Ilazy classes
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class LazyFactory<T>
    {
        /// <summary>
        /// creates a single thread lazy
        /// </summary>
        /// <param name="supplier"></param>
        /// <returns></returns>
        public static SingleThreadLazy<T> CreateSingleThreadLazy(Func<T> supplier)
        {
            //try catch
            return new SingleThreadLazy<T>(supplier);
        }

        /// <summary>
        /// creates multiple threads lazy
        /// </summary>
        /// <param name="supplier"></param>
        /// <returns></returns>
        public static MultipleThreadsLazy<T> CreateMultipleThreadsLazy(Func<T> supplier)
        {
            return new MultipleThreadsLazy<T>(supplier);
        }
    }
}
