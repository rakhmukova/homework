using System;
using System.Threading;

namespace Lazy
{
    public class MultipleThreadsLazy<T> : ILazy<T>
    {
        private Func<T> supplier;
        private T value;
        private bool isValueCreated;
        private Object lockObject = new();

        internal MultipleThreadsLazy(Func<T> supplier)
        {
            if (supplier == null)
            {
                throw new ArgumentNullException();
            }
            this.supplier = supplier;
        }

        public T Get()
        {
            if (!Volatile.Read(ref isValueCreated))
            {
                lock (lockObject)
                {
                    if (!Volatile.Read(ref isValueCreated))
                    {
                        value = supplier();
                        isValueCreated = true;
                        Volatile.Write(ref isValueCreated, true);
                    }

                }
            }
            return value;
        }
    }
}
