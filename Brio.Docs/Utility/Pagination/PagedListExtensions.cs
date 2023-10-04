using System;
using System.Linq;
using System.Linq.Expressions;

namespace Brio.Docs.Utility.Pagination
{
    internal static class PagedListExtensions
    {
        internal static IQueryable<T> ByPages<T, TKey>(this IQueryable<T> source,
            Expression<Func<T, TKey>> orderBy,
            int pageNumber,
            int pageSize,
            bool orderByDescending = false)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            source = orderByDescending ? source.OrderByDescending(orderBy) : source.OrderBy(orderBy);
            return source.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        }

        internal static IQueryable<T> ByPages<T>(this IOrderedQueryable<T> source, int pageNumber, int pageSize)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            return source.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        }
    }
}
