using System.Collections.Generic;

namespace Brio.Docs.Integration.Extensions
{
    public static class CollectionExtensions
    {
        public static void AddIsNotNull<T>(this ICollection<T> collection, T item)
        {
            if (item == null)
                return;

            collection.Add(item);
        }
    }
}
