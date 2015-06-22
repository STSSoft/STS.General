using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS.General.Collections
{
    public class PriorityQueue<T>
    {
        /// <summary>
        /// for each element at index i:
        /// - children are at indices 2i + 1 and 2i + 2
        /// - parent is at floor((i − 1) ∕ 2).
        /// </summary>
        private List<T> Items;

        public IComparer<T> Comparer { get; private set; }

        public int Count { get { return Items.Count; } }

        public bool IsEmpty { get { return Items.Count == 0; } }

        public PriorityQueue(IEnumerable<T> items, IComparer<T> comparer)
        {
            if (comparer == null)
                throw new ArgumentNullException("comparer");

            Comparer = comparer;

            Items = new List<T>(items);
            for (int i = (Items.Count / 2) - 1; i >= 0; i--)
                PercolateDown(i);
        }

        public PriorityQueue(IEnumerable<T> items)
            : this(items, Comparer<T>.Default)
        {
        }

        public PriorityQueue(IComparer<T> comparer)
            : this(Enumerable.Empty<T>(), comparer)
        {
        }

        public PriorityQueue()
            : this(Comparer<T>.Default)
        {
        }

        public void Enqueue(T item)
        {
            Items.Add(item);

            PercolateUp(Items.Count - 1);
        }

        public T Dequeue()
        {
            if (IsEmpty)
                throw new InvalidOperationException("Queue is empty.");

            T returnValue = Items[0];

            if (Items.Count > 1)
                Items[0] = Items[Items.Count - 1];

            Items.RemoveAt(Items.Count - 1);
            PercolateDown(0);

            return returnValue;
        }

        public T Peek 
        {
            get { return Items[0]; }
            set
            {
                Items[0] = value;
                PercolateDown(0);
            }
        }

        public T NextPeek
        {
            get
            {
                if (Count < 2)
                    throw new ArgumentException("Count < 2");

                return Items[GetMinIndex(1, 2)];
            }
        }

        private void PercolateUp(int index)
        {
            while (index > 0)
            {
                T item = Items[index];

                int parentIndex = (index - 1) / 2;
                T parent = Items[parentIndex];

                if (Comparer.Compare(item, parent) >= 0)
                    break;

                Items[index] = parent;
                Items[parentIndex] = item;

                index = parentIndex;
            }
        }

        private void PercolateDown(int index)
        {
            while (index < Items.Count - 1)
            {
                int minIndex = GetMinIndex(index, 2 * index + 1);
                minIndex = GetMinIndex(minIndex, 2 * index + 2);

                if (minIndex == index)
                    break;

                T temp = Items[index];
                Items[index] = Items[minIndex];
                Items[minIndex] = temp;

                index = minIndex;
            }
        }

        private int GetMinIndex(int i, int j)//i always exists
        {
            if (j >= Items.Count)
                return i;

            return Comparer.Compare(Items[i], Items[j]) <= 0 ? i : j;
        }
    }
}
