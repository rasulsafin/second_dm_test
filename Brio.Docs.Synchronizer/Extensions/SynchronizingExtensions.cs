using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Brio.Docs.Common.Extensions;
using Brio.Docs.Database;
using Brio.Docs.Synchronization.Interfaces;
using Brio.Docs.Synchronization.Models;

namespace Brio.Docs.Synchronization.Extensions
{
    internal static class SynchronizingExtensions
    {
        public static SynchronizingAction DetermineAction<T>(this SynchronizingTuple<T> tuple)
            => DetermineAction((tuple.Local, tuple.Synchronized, tuple.Remote));

        public static SynchronizingAction DetermineAction<T>(this SynchronizationTupleUnloaded<T> tuple)
            => DetermineAction((tuple.LocalId, tuple.SynchronizedId, tuple.Remote));

        public static SynchronizingAction DetermineAction(this (object local, object synchronized, object remote) tuple)
            => tuple.synchronized == null && tuple.local == null     ? SynchronizingAction.AddToLocal
                : tuple.synchronized == null && tuple.remote == null ? SynchronizingAction.AddToRemote
                : tuple.local == null && tuple.remote == null        ? SynchronizingAction.RemoveFromLocal
                : tuple.local == null                                ? SynchronizingAction.RemoveFromRemote
                : tuple.remote == null                               ? SynchronizingAction.RemoveFromLocal
                                                                       : SynchronizingAction.Merge;

        public static T GetRelevant<T>(
            this SynchronizingTuple<T> tuple,
            DateTime localUpdatedAt,
            DateTime remoteUpdatedAt)
            where T : class
            => GetRelevantValue(localUpdatedAt, remoteUpdatedAt, tuple.Local, tuple.Remote, tuple.Synchronized);

        public static void Merge<T>(
            this SynchronizingTuple<T> tuple,
            params Expression<Func<T, object>>[] properties)
            where T : class, ISynchronizable<T>, new()
        {
            MergePrivate(
                tuple,
                tuple.Local?.UpdatedAt ?? default,
                tuple.Remote?.UpdatedAt ?? default,
                properties.Select(GetLastPropertyInfo).ToArray());
            tuple.LinkEntities();
        }

        public static void Merge<T>(
            this SynchronizingTuple<T> tuple,
            DateTime localUpdatedAt,
            DateTime remoteUpdatedAt,
            params Expression<Func<T, object>>[] properties)
            where T : class, new()
            => MergePrivate(tuple, localUpdatedAt, remoteUpdatedAt, properties.Select(GetLastPropertyInfo).ToArray());

        [Obsolete]
        public static object GetPropertyValue<T>(this SynchronizingTuple<T> tuple, string propertyName)
        {
            var propertyInfo = typeof(T).GetProperty(propertyName);
            if (propertyInfo == null)
                throw new ArgumentException(nameof(GetPropertyValue), nameof(propertyName));

            return GetPropertyValuePrivate<T, object>(tuple, propertyInfo);
        }

        public static TProperty GetPropertyValue<T, TProperty>(this SynchronizingTuple<T> tuple, Expression<Func<T, TProperty>> property)
        {
            var propertyInfo = property.ToPropertyInfo();
            return GetPropertyValuePrivate<T, TProperty>(tuple, propertyInfo);
        }

        public static void SynchronizeChanges(this ISynchronizationChanges parentTuple, ISynchronizationChanges childTuple)
        {
            parentTuple.LocalChanged |= childTuple.LocalChanged;
            parentTuple.SynchronizedChanged |= childTuple.SynchronizedChanged;
            parentTuple.RemoteChanged |= childTuple.RemoteChanged;
        }

        private static PropertyInfo GetLastPropertyInfo<T>(Expression<Func<T, object>> property)
        {
            var expression = property.Body;

            if (expression is UnaryExpression { NodeType: ExpressionType.Convert } unaryExpression)
                expression = unaryExpression.Operand;

            if (expression is not MemberExpression { Member: PropertyInfo propertyInfo })
                throw new ArgumentException("The lambda expression must use properties only", nameof(property));

            return propertyInfo;
        }

        private static TProperty GetPropertyValuePrivate<T, TProperty>(
            SynchronizingTuple<T> tuple,
            PropertyInfo propertyInfo)
            => tuple.AsEnumerable()
               .Where(x => x != null)
               .Select(x => (TProperty)propertyInfo.GetValue(x))
               .FirstOrDefault(x => !EqualityComparer<TProperty>.Default.Equals(x, default));

        private static T GetRelevantValue<T>(
            DateTime localUpdatedAt,
            DateTime remoteUpdatedAt,
            T localValue,
            T remoteValue,
            T synchronizedValue)
        {
            var comparer = EqualityComparer<T>.Default;
            var localSynchronizedAndNotChanged = comparer.Equals(localValue, remoteValue) || comparer.Equals(synchronizedValue, remoteValue);
            var localNotChanged = comparer.Equals(synchronizedValue, localValue);
            var localMoreRelevant = localUpdatedAt > remoteUpdatedAt;

            var value = localSynchronizedAndNotChanged ? localValue
                : localNotChanged                      ? remoteValue
                : localMoreRelevant                    ? localValue
                                                         : remoteValue;
            return value;
        }

        private static void UpdateValue<T>(T obj, PropertyInfo property, object oldValue, object newValue, Action action)
        {
            if (!Equals(oldValue, newValue))
            {
                property.SetValue(obj, newValue);
                action();
            }
        }

        private static void UpdateValue<T>(
            SynchronizingTuple<T> tuple,
            PropertyInfo property,
            (object local, object synhronzied, object remote) oldValues,
            object value)
        {
            UpdateValue(tuple.Local, property, oldValues.local, value, () => tuple.LocalChanged = true);
            UpdateValue(
                tuple.Synchronized,
                property,
                oldValues.synhronzied,
                value,
                () => tuple.SynchronizedChanged = true);
            UpdateValue(tuple.Remote, property, oldValues.remote, value, () => tuple.RemoteChanged = true);
        }

        private static void MergePrivate<T>(
            SynchronizingTuple<T> tuple,
            DateTime localUpdatedAt,
            DateTime remoteUpdatedAt,
            PropertyInfo[] propertiesToMerge = null)
            where T : class, new()
        {
            var properties = typeof(T).GetProperties();

            if (propertiesToMerge != null)
                properties = propertiesToMerge.Where(x => properties.Contains(x)).ToArray();

            var isLocalRelevant = tuple.Local != null && tuple.Remote == null;
            var isRemoteRelevant = tuple.Remote != null && tuple.Local == null;

            tuple.Local ??= new T();
            tuple.Remote ??= new T();
            tuple.Synchronized ??= new T();

            foreach (var property in properties)
            {
                var synchronizedValue = property.GetValue(tuple.Synchronized);
                var localValue = property.GetValue(tuple.Local);
                var remoteValue = property.GetValue(tuple.Remote);

                object value;

                if (isLocalRelevant)
                {
                    value = localValue;
                }
                else if (isRemoteRelevant)
                {
                    value = remoteValue;
                }
                else
                {
                    value = GetRelevantValue(
                        localUpdatedAt,
                        remoteUpdatedAt,
                        localValue,
                        remoteValue,
                        synchronizedValue);
                }

                UpdateValue(tuple, property, (localValue, synchronizedValue, remoteValue), value);
            }
        }

        private static void LinkEntities<T>(this SynchronizingTuple<T> tuple)
            where T : class, ISynchronizable<T>
        {
            tuple.Synchronized.IsSynchronized = true;
            var externalID = tuple.Remote.ExternalID ?? tuple.ExternalID;
            tuple.Local.ExternalID = tuple.Synchronized.ExternalID = externalID;
            tuple.Remote.ExternalID ??= externalID;
            tuple.Local.SynchronizationMate = tuple.Synchronized;
        }
    }
}
