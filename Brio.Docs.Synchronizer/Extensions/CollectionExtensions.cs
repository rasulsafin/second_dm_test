using System;
using System.Collections.Generic;
using System.Linq;

namespace Brio.Docs.Synchronization.Extensions
{
    internal static class CollectionExtensions
    {
        public static IEnumerable<T> OrderByParent<T>(this IEnumerable<T> ie, Func<T, T> getParentFunc)
        {
            var result = new List<T>();
            var list = ie.ToList();

            foreach (var item in list.Where(x => getParentFunc(x) == null).ToArray())
            {
                result.Add(item);
                list.Remove(item);
            }

            while (list.Count > 0)
            {
                var first = list.FirstOrDefault(x => result.Contains(getParentFunc(x))) ?? list.First();
                result.Add(first);
                list.Remove(first);
            }

            return result;
        }
    }
}
