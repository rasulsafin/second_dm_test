using System.Collections.Generic;
using System.Linq;

namespace Brio.Docs.HttpConnection.Models
{
    public static class ObjectivesSortExtensions
    {
        public static bool IsDefault(this IReadonlyObjectivesSort sort)
        {
            if (sort.Sorts == null || sort.Sorts.Count == 0)
                return true;

            var defaultSort = ObjectivesSort.Default;
            return defaultSort.Sorts.SequenceEqual(sort.Sorts, new SortParameterEqualityComparer());
        }

        public static bool IsValid(this IReadonlyObjectivesSort sort)
        {
            // Sort fields should be defined exactly once
            return sort.Sorts.GroupBy(x => x.FieldName).Select(x => x.Count()).All(x => x == 1);
        }

        public static void CopyTo(this IReadonlyObjectivesSort from, ObjectivesSort to)
        {
            to.Sorts?.Clear();
            to.Sorts = from.Sorts.Select(x => new ObjectivesSortParameter
            {
                FieldName = x.FieldName,
                IsDescending = x.IsDescending,
            }).ToList();
        }

        private class SortParameterEqualityComparer : IEqualityComparer<ISortParameter>
        {
            public bool Equals(ISortParameter x, ISortParameter y)
                => string.Equals(x.FieldName, y.FieldName) && x.IsDescending == y.IsDescending;

            public int GetHashCode(ISortParameter obj)
                => obj.FieldName.GetHashCode() ^ obj.IsDescending.GetHashCode();
        }
    }
}
