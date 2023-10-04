using System;
using System.Collections.Generic;
using System.Linq;
using Brio.Docs.Synchronization.Models;

namespace Brio.Docs.Synchronization.Utils
{
    internal static class TuplesUtils
    {
        internal static List<SynchronizingTuple<T>> CreateSynchronizingTuples<T>(
            IEnumerable<T> local,
            IEnumerable<T> synchronized,
            IEnumerable<T> remote,
            Func<T, SynchronizingTuple<T>, bool> isEqualsFunc)
        {
            var result = new List<SynchronizingTuple<T>>();

            void AddToList(IEnumerable<T> list, Action<SynchronizingTuple<T>, T> set)
            {
                foreach (var element in list)
                {
                    var containsItem = result.FirstOrDefault(x => isEqualsFunc(element, x));
                    if (containsItem == null)
                        result.Add(containsItem = new SynchronizingTuple<T>());

                    set(containsItem, element);
                }
            }

            AddToList(local, (tuple, item) => tuple.Local = item);
            AddToList(synchronized, (tuple, item) => tuple.Synchronized = item);
            AddToList(remote, (tuple, item) => tuple.Remote = item);

            return result;
        }

        internal static List<(T, T, T)> CreateTuples<T>(
            IEnumerable<T> local,
            IEnumerable<T> synchronized,
            IEnumerable<T> remote,
            Func<T, T, bool> areEqual)
            where T : class
        {
            var result = new List<(T local, T synchronized, T remote)>();
            result.AddRange(
                local.Select(
                    item => (item, synchronized.FirstOrDefault(x => areEqual(item, x)),
                        remote.FirstOrDefault(x => areEqual(item, x)))));
            result.AddRange(
                synchronized.Where(x => result.All(r => !ReferenceEquals(r.synchronized, x)))
                   .Select<T, (T, T, T)>(item => (null, item, remote.FirstOrDefault(x => areEqual(item, x)))));
            result.AddRange(
                remote.Where(x => result.All(r => !ReferenceEquals(r.remote, x)))
                   .Select<T, (T, T, T)>(item => (null, null, item)));
            return result;
        }
    }
}
