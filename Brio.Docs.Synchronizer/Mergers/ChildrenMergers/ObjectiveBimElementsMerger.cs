using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Brio.Docs.Database;
using Brio.Docs.Database.Models;
using Brio.Docs.Synchronization.Extensions;
using Brio.Docs.Synchronization.Interfaces;
using Brio.Docs.Synchronization.Models;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Synchronization.Mergers.ChildrenMergers
{
    internal class ObjectiveBimElementsMerger : AChildrenMerger<Objective, BimElementObjective, BimElement>
    {
        private readonly Expression<Func<BimElementObjective, BimElement>> childFromLinkExpression = link => link.BimElement;
        private readonly Expression<Func<Objective, ICollection<BimElementObjective>>> collectionExpression = objective => objective.BimElements;

        public ObjectiveBimElementsMerger(
            DMContext context,
            IMerger<BimElement> merger,
            IAttacher<BimElement> attacher,
            ILogger<ObjectiveBimElementsMerger> logger)
            : base(context, merger, logger, attacher)
        {
        }

        protected override Expression<Func<BimElementObjective, BimElement>> ChildFromLinkExpression
            => childFromLinkExpression;

        protected override Expression<Func<Objective, ICollection<BimElementObjective>>> CollectionExpression
            => collectionExpression;

        protected override bool DoesNeedInTuple(BimElement child, SynchronizingTuple<BimElement> childTuple)
            => childTuple.Any(element => Equals(element, child));

        protected override Expression<Func<BimElement, bool>> GetNeedToRemoveExpression(Objective parent)
            => element => element.Objectives.All(
                x => x.Objective == parent ||
                    (x.Objective.SynchronizationMateID != null && x.Objective.SynchronizationMateID == parent.ID) ||
                    (parent.SynchronizationMateID != null && x.Objective.ID == parent.SynchronizationMateID));

        private bool Equals(BimElement x, BimElement y)
        {
            if (ReferenceEquals(x, null))
                return false;

            if (ReferenceEquals(y, null))
                return false;

            if (ReferenceEquals(x, y))
                return true;

            return string.Equals(x.GlobalID, y.GlobalID, StringComparison.Ordinal) &&
                string.Equals(x.ParentName, y.ParentName, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
