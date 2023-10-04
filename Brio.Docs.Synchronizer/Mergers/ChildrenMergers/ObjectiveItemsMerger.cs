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
    internal class ObjectiveItemsMerger : AChildrenMerger<Objective, ObjectiveItem, Item>
    {
        private readonly Expression<Func<Objective, ICollection<ObjectiveItem>>> collectionExpression =
            objective => objective.Items;

        private readonly Expression<Func<ObjectiveItem, Item>> synchronizableChildExpression = link => link.Item;

        public ObjectiveItemsMerger(
            DMContext context,
            IMerger<Item> childMerger,
            IAttacher<Item> attacher,
            ILogger<ObjectiveItemsMerger> logger)
            : base(context, childMerger, logger, attacher)
        {
            logger.LogTrace("ObjectiveItemsMerger created");
        }

        protected override Expression<Func<Objective, ICollection<ObjectiveItem>>> CollectionExpression
            => collectionExpression;

        protected override Expression<Func<ObjectiveItem, Item>> ChildFromLinkExpression
            => synchronizableChildExpression;

        protected override bool DoesNeedInTuple(Item child, SynchronizingTuple<Item> childTuple)
            => childTuple.DoesNeed(child) ||
                child.RelativePath == (string)childTuple.GetPropertyValue(nameof(Item.RelativePath));

        protected override Expression<Func<Item, bool>> GetNeedToRemoveExpression(Objective parent)
            => item => item.Objectives.All(x => x.Objective == parent) &&
                item.Project == null &&
                item.ProjectID == null;
    }
}
