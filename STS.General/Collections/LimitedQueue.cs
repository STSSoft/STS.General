using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STS.General.Collections
{
    public class LimitedQueue<T> : IEnumerable<T>
    {
        private T[] buffer;
        private int index = 0;
        public int Count { get; private set; }

        public LimitedQueue(int capacity)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException("capacity");

            buffer = new T[capacity];
        }

        public int Capacity 
        { 
            get { return buffer.Length; }
        }

        public void Enqueue(T item)
        {
            int idx = (index + Count) % Capacity;
            buffer[idx] = item;
            if (Count < Capacity)
                Count++;
            else
                index = (index + 1) % Capacity;
        }

        public T Dequeue()
        {
            if (Count == 0)
                throw new ArgumentOutOfRangeException();

            T item = buffer[index];
            index = (index + 1) % Capacity;
            Count--;

            return item;
        }

        public void Clear()
        {
            Count = 0;
        }

        public T First
        {
            get
            {
                if (Count == 0)
                    throw new ArgumentOutOfRangeException();

                return buffer[index];
            }
        }

        public T Last
        {
            get
            {
                if (Count == 0)
                    throw new ArgumentOutOfRangeException();

                return buffer[(index + Count - 1) % Capacity];
            }
        }

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException("index");

                return buffer[(this.index + index) % Capacity];
            }
            set
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException("index");

                buffer[(this.index + index) % Capacity] = value;
            }
        }

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
                yield return buffer[(index + i) % Capacity];
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        public LimitedQueue<T> Clone()
        {
            LimitedQueue<T> result = new LimitedQueue<T>(Capacity);
            result.index = this.index;
            result.Count = this.Count;

            Array.Copy(this.buffer, result.buffer, Capacity);
            return result;
        }
    }
}
