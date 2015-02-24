using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STS.General.Extensions;
using System.Collections;
using System.Diagnostics;

namespace STS.General.Collections
{
    public class DuplicateSet<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private readonly SortedSet<Node> Set;

        public readonly IComparer<TKey> Comparer;
        public readonly IEqualityComparer<TValue> EqualityComparer;

        public int Count { get; private set; }

        public DuplicateSet()
            : this(Comparer<TKey>.Default, EqualityComparer<TValue>.Default)
        {
        }

        public DuplicateSet(IComparer<TKey> comparer)
            : this(comparer, EqualityComparer<TValue>.Default)
        {
        }

        public DuplicateSet(IComparer<TKey> comparer, IEqualityComparer<TValue> equalityComparer)
        {
            Comparer = comparer;
            EqualityComparer = equalityComparer;

            Set = new SortedSet<Node>(new NodeComparer(comparer));
        }

        public bool Add(TKey key, TValue value)
        {
            var node = new Node(EqualityComparer, key);

            Node foundNode = null;
            if (!Set.TryGetValue(node, out foundNode))
            {
                Set.Add(node);
                foundNode = node;
            }

            if (!foundNode.Add(value))
                return false;

            Count++;

            return true;
        }

        public bool Remove(TKey key, TValue value)
        {
            var node = new Node(EqualityComparer, key);

            Node foundNode = null;
            if (!Set.TryGetValue(node, out foundNode))
                return false;

            if (!foundNode.Remove(value))
                return false;

            Count--;

            if (foundNode.Count == 0)
                Set.Remove(node);

            return true;
        }

        public bool Contains(TKey key)
        {
            Node keyNode = new Node(EqualityComparer, key);

            return Set.Contains(keyNode);
        }

        public void Clear()
        {
            Set.Clear();
        }

        public IEnumerable<KeyValuePair<TKey, TValue>> GetViewBetween(TKey from, TKey to)
        {
            return GetViewBetween(from, true, to, true);
        }

        public IEnumerable<KeyValuePair<TKey, TValue>> GetViewBetween(TKey from, bool hasFrom, TKey to, bool hasTo)
        {
            var fromNode = new Node(EqualityComparer, from);
            var toNode = new Node(EqualityComparer, to);

            var view = Set.GetViewBetween(fromNode, toNode, hasFrom, hasTo);

            foreach (var node in view)
            {
                foreach (var value in node)
                    yield return new KeyValuePair<TKey, TValue>(node.Key, value);
            }
        }

        public IEnumerable<KeyValuePair<TKey, TValue>> GetViewBetweenReverse(TKey from, TKey to)
        {
            return GetViewBetweenReverse(from, true, to, true);
        }

        public IEnumerable<KeyValuePair<TKey, TValue>> GetViewBetweenReverse(TKey from, bool hasFrom, TKey to, bool hasTo)
        {
            var fromNode = new Node(EqualityComparer, from);
            var toNode = new Node(EqualityComparer, to);

            var view = Set.GetViewBetween(fromNode, toNode, hasFrom, hasTo).Reverse();

            foreach (var node in view)
            {
                foreach (var value in node)
                    yield return new KeyValuePair<TKey, TValue>(node.Key, value);
            }
        }

        public bool FindNext(TKey key, out TValue value)
        {
            value = default(TValue);

            Node node;
            if (Set.FindNext(new Node(EqualityComparer, key), out node))
            {
                value = node.First();

                return true;
            }

            return false;
        }

        public bool FindPrev(TKey key, out TValue value)
        {
            value = default(TValue);

            Node node;
            if (Set.FindPrev(new Node(EqualityComparer, key), out node))
            {
                value = node.First();

                return true;
            }

            return false;
        }

        public bool FindAfter(TKey key, out TValue value)
        {
            value = default(TValue);

            Node node;
            if (Set.FindAfter(new Node(EqualityComparer, key), out node))
            {
                value = node.First();

                return true;
            }

            return false;
        }

        public bool FindBefore(TKey key, out TValue value)
        {
            value = default(TValue);

            Node node;
            if (Set.FindBefore(new Node(EqualityComparer, key), out node))
            {
                value = node.First();

                return true;
            }

            return false;
        }

        public int GetNumberOfValues(TKey key)
        {
            Node node;
            if (!Set.TryGetValue(new Node(EqualityComparer, key), out node))
                return 0;

            return node.Count;
        }

        public TValue Min { get { return Values.First(); } }
        public TValue Max { get { return GetViewBetweenReverse(default(TKey), false, default(TKey), false).First().Value; } }

        public IEnumerable<TValue> Values
        {
            get
            {
                foreach (var node in Set)
                {
                    foreach (var value in node)
                        yield return value;
                }
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (var node in Set)
            {
                foreach (var value in node)
                    yield return new KeyValuePair<TKey, TValue>(node.Key, value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private class Node : IEnumerable<TValue>
        {
            private TValue Value;
            private HashSet<TValue> Values;

            public readonly IEqualityComparer<TValue> EqualityComparer;
            public readonly TKey Key;

            public int Count { get; private set; }

            public Node(IEqualityComparer<TValue> equalityComparer, TKey key)
            {
                EqualityComparer = equalityComparer;
                Key = key;
            }

            public bool Add(TValue value)
            {
                if (Count == 0)
                {
                    Value = value;
                    Count = 1;

                    return true;
                }

                if (Count == 1)
                {
                    if (EqualityComparer.Equals(Value, value))
                        return false;

                    Values = new HashSet<TValue>();
                    Values.Add(Value);
                    Value = default(TValue);
                }

                if (!Values.Add(value))
                    return false;

                Count++;

                return true;
            }

            public bool Remove(TValue value)
            {
                if (Count == 0)
                    return false;

                if (Count == 1)
                {
                    if (!EqualityComparer.Equals(Value, value))
                        return false;

                    Value = default(TValue);
                    Count = 0;

                    return true;
                }

                if (!Values.Remove(value))
                    return false;

                if (Values.Count == 1)
                {
                    Value = Values.First();
                    Values = null;
                }

                Count--;

                return true;
            }

            public IEnumerator<TValue> GetEnumerator()
            {
                if (Count == 0)
                    yield break;

                if (Count == 1)
                    yield return Value;
                else
                {
                    foreach (var value in Values)
                        yield return value;
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }

        private class NodeComparer : IComparer<Node>
        {
            public readonly IComparer<TKey> Comparer;

            public NodeComparer(IComparer<TKey> comparer)
            {
                Comparer = comparer;
            }

            public int Compare(Node x, Node y)
            {
                return Comparer.Compare(x.Key, y.Key);
            }
        }
    }
}