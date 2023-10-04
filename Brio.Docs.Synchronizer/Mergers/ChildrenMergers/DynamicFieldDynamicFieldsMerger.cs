using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Brio.Docs.Database;
using Brio.Docs.Database.Models;
using Brio.Docs.Synchronization.Interfaces;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Synchronization.Mergers.ChildrenMergers
{
    internal class DynamicFieldDynamicFieldsMerger : ASimpleChildrenMerger<DynamicField, DynamicField>
    {
        private readonly Expression<Func<DynamicField, ICollection<DynamicField>>> collectionExpression =
            field => field.ChildrenDynamicFields;

        public DynamicFieldDynamicFieldsMerger(
            DMContext context,
            ILogger<DynamicFieldDynamicFieldsMerger> logger,
            IMerger<DynamicField> childMerger)
            : base(context, childMerger, logger)
        {
            logger.LogTrace("DynamicFieldDynamicFieldsMerger created");
        }

        protected override Expression<Func<DynamicField, ICollection<DynamicField>>> CollectionExpression
            => collectionExpression;
    }
}
