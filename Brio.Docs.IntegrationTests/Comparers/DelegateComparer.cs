using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Brio.Docs.Tests
{
    internal class DelegateComparer<T> : IEqualityComparer<T>
        where T : class
    {
        private readonly Func<T, T, bool> comparer;
        private readonly Func<T, int> hasher;

        public DelegateComparer(Func<T, T, bool> equalityComparer, Func<T, int> hasher = null)
        {
            this.comparer = equalityComparer;
            this.hasher = hasher;
        }

        public bool Equals([AllowNull] T x, [AllowNull] T y)
        {
            if ((x == null) != (y == null))
                return false;
            if (x == null)
                return true;
            return comparer(x, y);
        }

        public int GetHashCode([DisallowNull] T obj)
        {
            if (hasher != null)
                return hasher(obj);
            else
                return obj.GetHashCode();
        }
    }
}
