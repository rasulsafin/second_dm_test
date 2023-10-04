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
    internal class ProjectItemsMerger : ASimpleChildrenMerger<Project, Item>
    {
        private readonly Expression<Func<Project, ICollection<Item>>> collectionExpression = project => project.Items;
        private readonly Expression<Func<Item, bool>> needToRemoveExpression = item => !item.Objectives.Any();

        public ProjectItemsMerger(
            DMContext context,
            IMerger<Item> childMerger,
            IAttacher<Item> attacher,
            ILogger<ProjectItemsMerger> logger)
            : base(context, childMerger, logger, attacher)
        {
            logger.LogTrace("ProjectItemsMerger created");
        }

        protected override Expression<Func<Project, ICollection<Item>>> CollectionExpression => collectionExpression;

        protected override bool DoesNeedInTuple(Item child, SynchronizingTuple<Item> childTuple)
            => childTuple.DoesNeed(child) ||
                child.RelativePath == (string)childTuple.GetPropertyValue(nameof(Item.RelativePath));

        protected override Expression<Func<Item, bool>> GetNeedToRemoveExpression(Project parent)
            => needToRemoveExpression;

        protected override bool UnlinkChild(Project parent, Item child)
        {
            if (base.UnlinkChild(parent, child))
            {
                child.ProjectID = null;
                return true;
            }

            return false;
        }
    }
}
