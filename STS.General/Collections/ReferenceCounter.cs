using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;

namespace STS.General.Collections
{
    public class ReferenceCounter<T>
    {
        private Dictionary<T, Item> map = new Dictionary<T, Item>();

        public T Obtain(T reference)
        {
            Item item;
            lock (map)
            {
                if (!map.TryGetValue(reference, out item))
                    map[reference] = item = new Item(reference);

                item.RefCount++;
            }

            return item.Reference;
        }

        public void Release(T reference)
        {
            Item item;
            lock (map)
            {
                if (map.TryGetValue(reference, out item))
                {
                    item.RefCount--;

                    if (item.RefCount <= 0)
                        map.Remove(reference);
                }
            }
        }

        private class Item
        {
            public readonly T Reference;
            public int RefCount;

            public Item(T reference)
            {
                Reference = reference;
            }
        }
    }
}
              
