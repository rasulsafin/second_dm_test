using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Brio.Docs.Database;
using Brio.Docs.Database.Models;
using Brio.Docs.Synchronization.Interfaces;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Synchronization.Mergers.ChildrenMergers
{
    internal class ObjectiveDynamicFieldsMerger : ASimpleChildrenMerger<Objective, DynamicField>
    {
        private readonly Expression<Func<Objective, ICollection<DynamicField>>> collectionExpression =
            objective => objective.DynamicFields;

        public ObjectiveDynamicFieldsMerger(
            DMContext context,
            IMerger<DynamicField> childMerger,
            ILogger<ObjectiveDynamicFieldsMerger> logger)
            : base(context, childMerger, logger)
        {
            logger.LogTrace("ObjectiveDynamicFieldsMerger created");
        }

        protected override Expression<Func<Objective, ICollection<DynamicField>>> CollectionExpression
            => collectionExpression;
    }
}
