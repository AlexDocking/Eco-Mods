using System;
using System.Collections.Generic;

namespace CompatibleTools
{
    public class NumberComparer<T> : IComparer<T>
    {
        private Func<T, float> KeyGenerator { get; }
        public NumberComparer(Func<T, float> keyGenerator)
        {
            KeyGenerator = keyGenerator ?? throw new ArgumentNullException(nameof(keyGenerator));
        }
        public int Compare(T x, T y) => KeyGenerator(x).CompareTo(KeyGenerator(y));
    }
}
