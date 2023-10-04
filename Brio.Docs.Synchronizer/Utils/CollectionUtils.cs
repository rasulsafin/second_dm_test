using System;
using System.Collections.Generic;
using System.Linq;
using Brio.Docs.Database;
using Brio.Docs.Synchronization.Extensions;
using Brio.Docs.Synchronization.Models;

namespace Brio.Docs.Synchronization.Utils
{
    internal static class CollectionUtils
    {
        public static void Merge<TCollection, T, TParent>(
            SynchronizingTuple<TParent> tuple,
            ICollection<TCollection> local,
            ICollection<TCollection> synchronized,
            ICollection<TCollection> remote,
            Func<T, T, bool> areEqual,
            Func<T, TParent, TCollection> ctor,
            Func<TCollection, T> selectFunc)
            where T : class
            where TParent : ISynchronizable<TParent>
        {
            var bimElements = TuplesUtils.CreateTuples(
                local.Select(selectFunc),
                synchronized.Select(selectFunc),
                remote.Select(selectFunc),
                areEqual);

            foreach (var elements in bimElements)
            {
                var item = elements.Item1 ?? elements.Item2 ?? elements.Item3;

                switch (elements.DetermineAction())
                {
                    case SynchronizingAction.Merge:
                        tuple.SynchronizedChanged = true;
                        if (elements.Item2 == null)
                            synchronized.Add(ctor(elements.Item1 ?? elements.Item3, tuple.Synchronized));
                        break;
                    case SynchronizingAction.AddToLocal:
                        local.Add(ctor(item, tuple.Local));
                        synchronized.Add(ctor(item, tuple.Synchronized));
                        tuple.SynchronizedChanged = tuple.LocalChanged = true;
                        break;
                    case SynchronizingAction.AddToRemote:
                        remote.Add(ctor(item, tuple.Remote));
                        synchronized.Add(ctor(item, tuple.Synchronized));
                        tuple.SynchronizedChanged = tuple.RemoteChanged = true;
                        break;
                    case SynchronizingAction.RemoveFromLocal:
                        RemoveItem(local, selectFunc, elements.Item1, () => tuple.LocalChanged = true);
                        RemoveItem(synchronized, selectFunc, elements.Item2, () => tuple.SynchronizedChanged = true);
                        break;
                    case SynchronizingAction.RemoveFromRemote:
                        RemoveItem(remote, selectFunc, elements.Item3, () => tuple.RemoteChanged = true);
                        RemoveItem(synchronized, selectFunc, elements.Item2, () => tuple.SynchronizedChanged = true);
                        break;
                }
            }
        }

        private static void RemoveItem<TCollection, T>(
            ICollection<TCollection> collection,
            Func<TCollection, T> selectFunc,
            T item,
            Action action)
            where T : class
        {
            if (item != null)
            {
                collection.Remove(collection.First(x => ReferenceEquals(selectFunc(x), item)));
                action();
            }
        }
    }
}
