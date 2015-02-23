using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace STS.General.Comparers
{
    public class ComparerCounter<T> : IComparer<T>
    {
        private long count;

        public IComparer<T> Comparer;

        public ComparerCounter(IComparer<T> comparer)
        {
            Comparer = comparer;
        }

        public int Compare(T x, T y)
        {
            var cmp = Comparer.Compare(x, y);

            Interlocked.Increment(ref count);

            return cmp;
        }

        public void Reset()
        {
            Interlocked.Exchange(ref count, 0);
        }

        public long Count
        {
            get { return Interlocked.Read(ref count); }
        }
    }
}