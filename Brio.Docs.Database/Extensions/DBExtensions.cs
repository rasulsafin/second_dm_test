using System.Linq;

namespace Brio.Docs.Database.Extensions
{
    public static class DBExtensions
    {
        public static IQueryable<T> Synchronized<T>(this IQueryable<T> queryable)
                where T : ISynchronizable<T>
            => queryable.Where(x => x.IsSynchronized);

        public static IQueryable<T> Unsynchronized<T>(this IQueryable<T> queryable)
                where T : ISynchronizable<T>
            => queryable.Where(x => !x.IsSynchronized);
    }
}
