using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STS.General.Collections
{
    public class DoubleLimitedQueue<T>
    {
        private readonly object SyncRoot = new object();
        private LimitedQueue<T> buffer1;
        private LimitedQueue<T> buffer2;
        public readonly int Capacity;
        
        public DoubleLimitedQueue(int capacity)
        {
            buffer1 = new LimitedQueue<T>(capacity);
            buffer2 = new LimitedQueue<T>(capacity);
            Capacity = capacity;
        }

        public void Enqueue(T item)
        {
            lock (SyncRoot)
                buffer1.Enqueue(item);
        }

        public void Switch()
        {
            lock (SyncRoot)
            {
                var tmp = buffer1;
                buffer1 = buffer2;
                buffer2 = tmp;
            }
        }

        public T Dequeue()
        {
            return buffer2.Dequeue();
        }

        public int Count
        {
            get { return buffer2.Count; }
        }
    }
}
