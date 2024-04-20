// Copyright (c) Strange Loop Games. All rights reserved.
// See LICENSE file in the project root for full license information.

namespace Eco.Mods.TechTree
{
    using System.Collections.Generic;
    using System.Collections;

    public abstract partial class ShovelItem
    {
        private class SortedCollection<T> : ICollection<T>
        {
            public int Count => ((ICollection<T>)Values).Count;

            public bool IsReadOnly => ((ICollection<T>)Values).IsReadOnly;

            private List<T> Values { get; } = new List<T>();
            private IComparer<T> Comparer { get; }

            public SortedCollection(IComparer<T> comparer)
            {
                Comparer = comparer;
            }

            public void Add(T item)
            {
                ((ICollection<T>)Values).Add(item);
                Values.Sort(Comparer);
            }

            public void Clear()
            {
                ((ICollection<T>)Values).Clear();
            }

            public bool Contains(T item)
            {
                return ((ICollection<T>)Values).Contains(item);
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                ((ICollection<T>)Values).CopyTo(array, arrayIndex);
            }

            public IEnumerator<T> GetEnumerator()
            {
                return ((IEnumerable<T>)Values).GetEnumerator();
            }

            public bool Remove(T item)
            {
                return ((ICollection<T>)Values).Remove(item);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable)Values).GetEnumerator();
            }
        }
    }
}
