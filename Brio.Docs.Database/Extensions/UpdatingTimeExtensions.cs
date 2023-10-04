using System;
using System.Collections.Generic;
using System.Linq;
using Brio.Docs.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Brio.Docs.Database.Extensions
{
    public static class UpdatingTimeExtensions
    {
        public static void UpdateDateTime(this ChangeTracker changeTracker, DateTime dateTime = default)
        {
            dateTime = dateTime == default ? DateTime.UtcNow : dateTime;
            var hashset = new HashSet<ISynchronizableBase>();
            var entries = changeTracker.Entries().ToArray();

            foreach (var entry in entries)
            {
                if (entry.Entity is ISynchronizableBase synchronizable &&
                    entry.State is EntityState.Added or EntityState.Modified)
                    hashset.Add(synchronizable);

                if (entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
                {
                    foreach (ISynchronizableBase parent in GetParents(changeTracker.Context, entry.Entity)
                       .Where(parent => parent is ISynchronizableBase)
                       .TakeWhile(parent => !hashset.Contains(parent)))
                        hashset.Add(parent);
                }
            }

            foreach (var synchronizable in hashset)
                synchronizable.UpdatedAt = dateTime;
        }

        private static IEnumerable<object> GetParents(DbContext context, object entity)
        {
            while (entity != null)
            {
                object parent = null;

                switch (entity)
                {
                    case Item item:
                        parent = GetParent(context, item.Project, item.ProjectID);
                        break;
                    case DynamicField dynamicField:
                        parent = GetParent(context, dynamicField.ParentField, dynamicField.ParentFieldID);
                        parent ??= GetParent(context, dynamicField.Objective, dynamicField.ObjectiveID);
                        break;
                    case ObjectiveItem objectiveItem:
                        parent = GetParent(context, objectiveItem.Objective, objectiveItem.ObjectiveID);
                        break;
                    case BimElementObjective bimElementObjective:
                        parent = GetParent(context, bimElementObjective.Objective, bimElementObjective.ObjectiveID);
                        break;
                }

                if (parent != null)
                    yield return parent;

                entity = parent;
            }
        }

        private static T GetParent<T>(DbContext context, T parent, int? parentID)
            where T : class
            => parent ?? (parentID.HasValue ? context.Find<T>(parentID.Value) : null);
    }
}
