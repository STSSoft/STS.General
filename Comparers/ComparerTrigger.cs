using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace STS.General.Comparers
{
    public class ComparerTrigger<T> : IComparer<T>
    {
        public IComparer<T> Comparer;
        public Action<ComparerTrigger<T>> Action;

        public ComparerTrigger(IComparer<T> comparer, Action<ComparerTrigger<T>> action)
        {
            Comparer = comparer;
            Action = action;
        }

        public int Compare(T x, T y)
        {
            var cmp = Comparer.Compare(x, y);

            Action(this);

            return cmp;
        }
    }

    public class ComparerCounter<T> : ComparerTrigger<T>
    {
        private long count;

        public ComparerCounter(IComparer<T> comparer)
            : base(comparer, (x) => { Interlocked.Increment(ref count)})
        {
        }

        public void Reset()
        {
            Count = 0;
        }

        public long Count
        {
            get { return Interlocked.Read(ref count); }
            private set { Interlocked.Exchange(ref count, value); }
        }
    }
}
