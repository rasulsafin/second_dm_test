using System;
using System.Linq;
using System.Linq.Expressions;
using Brio.Docs.Client.Sorts;
using Microsoft.EntityFrameworkCore;

namespace Brio.Docs.Utility.Sorting
{
    public static class SortingQueryExtensions
    {
        /// <summary>
        /// Apply sorting to query specified by <paramref name="sortParameters"/>.
        /// </summary>
        /// <typeparam name="T">Sequence element type.</typeparam>
        /// <typeparam name="TKey">Default sort element type.</typeparam>
        /// <param name="source">Source query.</param>
        /// <param name="sortParameters">Sort parameters. May be null or empty - then default sorting is used.</param>
        /// <param name="defaultSort">Fallback sort used when <paramref name="sortParameters"/> are empty.</param>
        /// <param name="defaultByDescending">Invert direction of default sort.</param>
        /// <returns>Ordered query.</returns>
        public static IOrderedQueryable<T> SortWithParameters<T, TKey>(
            this IQueryable<T> source,
            SortParameters sortParameters,
            Expression<Func<T, TKey>> defaultSort,
            bool defaultByDescending = false)
        {
            if (sortParameters == null || sortParameters.Sorts == null || !sortParameters.Sorts.Any())
                return defaultByDescending ? source.OrderByDescending(defaultSort) : source.OrderBy(defaultSort);

            var firstSort = sortParameters.Sorts.ElementAt(0);
            var query = !firstSort.IsDescending
                ? source.OrderBy(x => EF.Property<object>(x, firstSort.FieldName))
                : source.OrderByDescending(x => EF.Property<object>(x, firstSort.FieldName));

            foreach (var sort in sortParameters.Sorts.Skip(1))
            {
                query = !sort.IsDescending
                ? query.ThenBy(x => EF.Property<object>(x, sort.FieldName))
                : query.ThenByDescending(x => EF.Property<object>(x, sort.FieldName));
            }

            return query;
        }

        public static IOrderedQueryable<T> SortWithParameters<T, TKey>(
            this IQueryable<T> source,
            SortParameters sortParameters,
            QueryMapper<T> mapper,
            Expression<Func<T, TKey>> defaultSort,
            bool defaultByDescending = false)
        {
            if (sortParameters == null || sortParameters.Sorts == null || !sortParameters.Sorts.Any())
                return defaultByDescending ? source.OrderByDescending(defaultSort) : source.OrderBy(defaultSort);

            var sorts = mapper.Config.IgnoreNotMappedFields
                ? sortParameters.Sorts.Where(x => mapper.HasMap(x.FieldName))
                : sortParameters.Sorts;

            if (!sorts.Any())
                return defaultByDescending ? source.OrderByDescending(defaultSort) : source.OrderBy(defaultSort);

            var firstSort = sorts.ElementAt(0);
            var query = !firstSort.IsDescending
                ? source.OrderBy(mapper.GetExpression(firstSort.FieldName))
                : source.OrderByDescending(mapper.GetExpression(firstSort.FieldName));

            foreach (var sort in sorts.Skip(1))
            {
                query = !sort.IsDescending
                    ? query.ThenBy(mapper.GetExpression(sort.FieldName))
                    : query.ThenByDescending(mapper.GetExpression(sort.FieldName));
            }

            return query;
        }
    }
}
