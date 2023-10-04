using System.Collections.Generic;
using System.Linq;
using Brio.Docs.Database.Models;
using Brio.Docs.Synchronization.Interfaces;

namespace Brio.Docs.Synchronization.Strategies
{
    internal class DynamicFieldExternalIdUpdater : IExternalIdUpdater<DynamicField>
    {
        private readonly IEqualityComparer<DynamicField> comparer;

        public DynamicFieldExternalIdUpdater()
            => comparer = new DynamicFieldComparer();

        public void UpdateExternalIds(IEnumerable<DynamicField> local, IEnumerable<DynamicField> remote)
        {
            var remotes = remote.ToList();

            foreach (var dynamicField in local)
                UpdateExternalIDs(remotes, dynamicField, comparer);
        }

        private static void UpdateExternalIDs(IEnumerable<DynamicField> remote, DynamicField dynamicField, IEqualityComparer<DynamicField> comparer)
        {
            var found = remote.FirstOrDefault(x => comparer.Equals(dynamicField, x));
            if (found == null)
                return;

            dynamicField.ExternalID = found.ExternalID;
            foreach (var child in dynamicField.ChildrenDynamicFields ?? Enumerable.Empty<DynamicField>())
                UpdateExternalIDs(found.ChildrenDynamicFields.ToList(), child, comparer);
        }

        private class DynamicFieldComparer : IEqualityComparer<DynamicField>
        {
            public bool Equals(DynamicField x, DynamicField y)
            {
                if (ReferenceEquals(x, y))
                    return true;

                if (ReferenceEquals(x, null))
                    return false;

                if (ReferenceEquals(y, null))
                    return false;

                if (x.GetType() != y.GetType())
                    return false;

                if (!string.IsNullOrWhiteSpace(x.ExternalID) && !string.IsNullOrWhiteSpace(y.ExternalID))
                    return x.ExternalID == y.ExternalID;

                var xFields = x.ChildrenDynamicFields ?? Enumerable.Empty<DynamicField>();
                var yFields = y.ChildrenDynamicFields ?? Enumerable.Empty<DynamicField>();
                return
                    x.Type == y.Type &&
                    x.Name == y.Name &&
                    x.Value == y.Value &&
                    (x.ChildrenDynamicFields?.Count ?? 0) == (y.ChildrenDynamicFields?.Count ?? 0) &&
                    xFields.All(xc => yFields.Any(yc => Equals(xc, yc)));
            }

            public int GetHashCode(DynamicField obj)
                => 0;
        }
    }
}
