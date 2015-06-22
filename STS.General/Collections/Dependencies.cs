using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STS.General.Collections
{
    public class Dependencies<T>
    {
        private readonly Dictionary<T, Node> map;

        public readonly IEqualityComparer<T> EqualityComparer;
        public readonly Action<T> OnRemoved;

        public Dependencies(IEqualityComparer<T> equalityComparer, Action<T> onRemoved)
        {
            if (equalityComparer == null)
                throw new ArgumentNullException("equalityComparer");

            EqualityComparer = equalityComparer;
            OnRemoved = onRemoved;

            map = new Dictionary<T, Node>(EqualityComparer);
        }

        public Dependencies(Action<T> onRemoved = null)
            : this(EqualityComparer<T>.Default, onRemoved)
        {
        }

        public Dependencies()
            : this(EqualityComparer<T>.Default, null)
        {
        }

		private void Delete(T item)
		{
			map.Remove(item);

			if (OnRemoved != null)
				OnRemoved(item);
		}

		public void Add(T item, params T[] references)
        {
            if (references.Length == 0)
                return;

            Node node;
            if (!map.TryGetValue(item, out node))
                map[item] = node = new Node(this, item);

            node.Add(references);
        }

        public void Remove(T item)
        {
            Node node;
            if (!map.TryGetValue(item, out node))
                return;

            node.ClearReferences();
        }

        public T[] GetReferences(T item)
        {
            Node node;
            if (!map.TryGetValue(item, out node))
                return new T[] { };

            return node.References.ToArray();
        }

        public int Count
        {
            get { return map.Count; }
        }

        private class Node
        {
            public readonly Dependencies<T> Owner;
            public readonly T Item;

            public readonly HashSet<T> References = new HashSet<T>();
            public int ReferralCount;

            public Node(Dependencies<T> owner, T item)
            {
                if (owner == null)
                    throw new ArgumentNullException("owner");

                Owner = owner;
                Item = item;
            }

            public void Add(T[] references)
            {
                foreach (var reference in references)
                {
                    if (!References.Add(reference))
                        continue;

                    Node node;
                    if (!Owner.map.TryGetValue(reference, out node))
                        Owner.map[reference] = node = new Node(Owner, reference);

                    node.ReferralCount++;
                }
            }

            public void ClearReferences()
            {
                foreach (var reference in References)
                {
                    Node node = Owner.map[reference];
                    node.ReferralCount--;

                    if (node.ReferralCount == 0 && node.References.Count == 0)
                        Owner.Delete(node.Item);
                }

                References.Clear();
                if (ReferralCount == 0)
                    Owner.Delete(Item);
            }
        }
    }
}
