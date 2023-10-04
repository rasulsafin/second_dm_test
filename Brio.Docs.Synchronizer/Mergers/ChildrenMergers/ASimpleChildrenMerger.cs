using System;
using System.Linq.Expressions;
using Brio.Docs.Database;
using Brio.Docs.Synchronization.Extensions;
using Brio.Docs.Synchronization.Interfaces;
using Brio.Docs.Synchronization.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Synchronization.Mergers.ChildrenMergers
{
    internal abstract class ASimpleChildrenMerger<TParent, TChild> : AChildrenMerger<TParent, TChild, TChild>
        where TParent : class
        where TChild : class, ISynchronizable<TChild>, new()
    {
        private readonly Expression<Func<TChild, TChild>> synchronizableChildExpression = child => child;

        protected ASimpleChildrenMerger(
            DbContext context,
            IMerger<TChild> childMerger,
            ILogger<ASimpleChildrenMerger<TParent, TChild>> logger,
            IAttacher<TChild> attacher = null)
            : base(context, childMerger, logger, attacher)
        {
            logger.LogTrace("Base initialization of simple children merger completed");
        }

        protected override Expression<Func<TChild, TChild>> ChildFromLinkExpression => synchronizableChildExpression;

        protected override bool DoesNeedInTuple(TChild child, SynchronizingTuple<TChild> childTuple)
            => childTuple.DoesNeed(child);
    }
}
