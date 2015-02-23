using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STSdb4.General.Threading
{
    public class ReferenceCounter<T>
    {
        private readonly object SyncRoot = new object();

        public int Counter { get; private set; }
        public T Object { get; private set; }

        public Func<T> Factory { get; private set; }

        public ReferenceCounter(Func<T> factory)
        {
            Factory = factory;
        }

        public void Obtain()
        {
            lock (SyncRoot)
            {
                Counter++;

                if (Counter == 1)
                    Object = Factory();
            }
        }

        public void Release()
        {
            lock (SyncRoot)
            {
                if (Counter == 0)
                    return;

                Counter--;

                if (Counter == 0)
                    Object = default(T);
            }
        }
    }
}
