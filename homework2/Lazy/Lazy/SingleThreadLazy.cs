using System;

namespace Lazy
{
    public class SingleThreadLazy<T> : ILazy<T>
    {
        private Func<T> supplier;
        private T value;
        private bool isValueCreated;
        internal SingleThreadLazy(Func<T> supplier)
        {
            if (supplier == null)
            {
                throw new ArgumentNullException();
            }
            this.supplier = supplier;
        }

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

