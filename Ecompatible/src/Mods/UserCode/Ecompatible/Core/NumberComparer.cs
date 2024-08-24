// Copyright (c) Alex Docking
//
// This file is part of Ecompatible.
//
// Ecompatible is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// Ecompatible is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License along with Ecompatible. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;

namespace Ecompatible
{
    internal class NumberComparer<T> : IComparer<T>
    {
        private Func<T, float> KeyGenerator { get; }
        public NumberComparer(Func<T, float> keyGenerator)
        {
            KeyGenerator = keyGenerator ?? throw new ArgumentNullException(nameof(keyGenerator));
        }
        public int Compare(T x, T y) => KeyGenerator(x).CompareTo(KeyGenerator(y));
    }
}
