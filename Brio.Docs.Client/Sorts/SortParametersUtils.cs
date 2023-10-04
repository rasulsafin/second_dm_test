using System.Collections.Generic;
using System.Linq;
using Brio.Docs.Client.Exceptions;

namespace Brio.Docs.Client.Sorts
{
    public static class SortParametersUtils
    {
        public static string ToQueryString(this SortParameters p)
        {
            if (p.Sorts == null || p.Sorts.Count == 0)
                return string.Empty;

            return string.Join(",", p.Sorts.Select(FormatSortParameter));
        }

        public static SortParameters FromQueryString(string sort)
        {
            if (string.IsNullOrEmpty(sort))
                return null;

            var sorts = new List<SortParameter>();
            var parts = sort.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in parts)
            {
                if (item.Contains(':'))
                {
                    var sortParts = item.Split(':');
                    var sortName = sortParts[0];
                    var sortDir = sortParts[1];
                    if (sortDir.Equals("asc", System.StringComparison.InvariantCultureIgnoreCase))
                    {
                        sorts.Add(new SortParameter { FieldName = sortName, IsDescending = false });
                    }
                    else if (sortDir.Equals("desc", System.StringComparison.InvariantCultureIgnoreCase))
                    {
                        sorts.Add(new SortParameter { FieldName = sortName, IsDescending = true });
                    }
                    else
                    {
                        throw new ArgumentValidationException(nameof(sort));
                    }
                }
                else
                {
                    sorts.Add(new SortParameter() { FieldName = item });
                }
            }

            return new SortParameters { Sorts = sorts };
        }

        private static string FormatSortParameter(SortParameter p)
            => p.IsDescending ? $"{p.FieldName}:desc" : p.FieldName;
    }
}
