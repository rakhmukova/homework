namespace Lazy
{
    /// <summary>
    /// An interface for lazy initialization
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ILazy<T>
    {
        /// <summary>
        /// Returns the lazily initialized value of the current Lazy<T> instance
        /// </summary>
        public T Get();
    }
}

