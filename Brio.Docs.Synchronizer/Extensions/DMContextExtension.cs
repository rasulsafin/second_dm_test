using System;
using System.Threading.Tasks;
using Brio.Docs.Database.Models;
using Brio.Docs.Synchronization.Models;
using Microsoft.EntityFrameworkCore;

namespace Brio.Docs.Synchronization.Extensions
{
    internal static class DMContextExtension
    {
        public static int GetId(this object entity)
            => entity switch
            {
                Project project => project.ID,
                DynamicField dynamicField => dynamicField.ID,
                Objective objective => objective.ID,
                Item item => item.ID,
                BimElement bimElement => bimElement.ID,
                _ => throw new NotSupportedException()
            };

        public static string GetRemoteId(this object entity)
            => entity switch
            {
                Project project => project.ExternalID,
                DynamicField dynamicField => dynamicField.ExternalID,
                Objective objective => objective.ExternalID,
                Item item => item.ExternalID,
                BimElement bimElement => bimElement.GlobalID,
                _ => throw new NotSupportedException()
            };

        public static async ValueTask<SynchronizingTuple<T>> Load<T>(
            this DbContext context,
            SynchronizationTupleUnloaded<T> tupleUnloaded)
            where T : class
        {
            var local = tupleUnloaded.LocalId == null
                ? null
                : await context.Set<T>().FindAsync(tupleUnloaded.LocalId).ConfigureAwait(false);
            var synchronized = tupleUnloaded.SynchronizedId == null
                ? null
                : await context.Set<T>().FindAsync(tupleUnloaded.SynchronizedId).ConfigureAwait(false);

            return new SynchronizingTuple<T>
            {
                Local = local,
                Synchronized = synchronized,
                Remote = tupleUnloaded.Remote,
            };
        }
    }
}
