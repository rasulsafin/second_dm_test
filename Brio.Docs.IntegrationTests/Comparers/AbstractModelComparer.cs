using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Brio.Docs.Tests
{
    internal abstract class AbstractModelComparer<T> : IEqualityComparer<T>
        where T : class
    {
        public AbstractModelComparer(bool ignoreIDs = false)
        {
            IgnoreIDs = ignoreIDs;
        }

        public bool IgnoreIDs { get; }

        public abstract bool NotNullEquals([DisallowNull] T x, [DisallowNull] T y);

        public virtual bool Equals([AllowNull] T x, [AllowNull] T y)
        {
            if ((x == null) != (y == null))
                return false;
            if (x == null)
                return true;
            return NotNullEquals(x, y);
        }

        public virtual int GetHashCode([DisallowNull] T obj) => obj.GetHashCode();
    }
}
