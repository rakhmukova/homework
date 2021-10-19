namespace Lazy
{
    /// <summary>
    /// An interface for lazy initialization
    /// </summary>
    public interface ILazy<out T>
    {
        /// <summary>
        /// Returns the lazily initialized value of the current Lazy<T> instance
        /// </summary>
        public T Get();
    }
}

