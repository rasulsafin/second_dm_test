using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Brio.Docs.Common.Extensions;
using Brio.Docs.Synchronization.Extensions;
using Brio.Docs.Synchronization.Interfaces;
using Brio.Docs.Synchronization.Models;
using Brio.Docs.Synchronization.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Synchronization.Mergers.ChildrenMergers
{
    internal abstract class AChildrenMerger<TParent, TLink, TChild>
        : IChildrenMerger<TParent, TChild>
        where TParent : class
        where TLink : class, new()
        where TChild : class
    {
        private readonly IAttacher<TChild> attacher;
        private readonly IMerger<TChild> childMerger;
        private readonly DbContext context;
        private readonly Expression<Func<TChild, bool>> defaultNeedToRemoveExpression = child => true;
        private readonly Lazy<PropertyInfo> lazyChildProperty;

        private readonly Lazy<PropertyInfo> lazyChildrenCollectionProperty;
        private readonly Lazy<Func<TLink, TChild>> lazyGetChildFunc;
        private readonly Lazy<Func<TParent, ICollection<TLink>>> lazyGetChildrenCollectionFunc;
        private readonly Lazy<Expression<Func<TParent, IEnumerable<TLink>>>> lazyGetChildrenEnumerableExpression;
        private readonly Lazy<bool> lazyIsOneToManyRelationship;

        private readonly ILogger<AChildrenMerger<TParent, TLink, TChild>> logger;

        protected AChildrenMerger(
            DbContext context,
            IMerger<TChild> childMerger,
            ILogger<AChildrenMerger<TParent, TLink, TChild>> logger,
            IAttacher<TChild> attacher = null)
        {
            this.context = context;
            this.childMerger = childMerger;
            this.logger = logger;
            this.attacher = attacher;

            lazyGetChildrenCollectionFunc =
                new Lazy<Func<TParent, ICollection<TLink>>>(() => CollectionExpression.Compile());
            lazyChildrenCollectionProperty = new Lazy<PropertyInfo>(() => CollectionExpression.ToPropertyInfo());
            lazyGetChildrenEnumerableExpression = new Lazy<Expression<Func<TParent, IEnumerable<TLink>>>>(
                () =>
                {
                    var parameter = Expression.Parameter(typeof(TParent));
                    return Expression.Lambda<Func<TParent, IEnumerable<TLink>>>(
                        Expression.Property(parameter, ChildrenCollectionProperty),
                        false,
                        parameter);
                });

            lazyIsOneToManyRelationship = new Lazy<bool>(
                () =>
                {
                    var expression = ChildFromLinkExpression.Body;

                    if (expression is UnaryExpression { NodeType: ExpressionType.Convert } unaryExpression)
                        expression = unaryExpression.Operand;

                    return expression is ParameterExpression;
                });

            lazyGetChildFunc = new Lazy<Func<TLink, TChild>>(
                () => ChildFromLinkExpression.Compile());
            lazyChildProperty = new Lazy<PropertyInfo>(() => ChildFromLinkExpression.ToPropertyInfo());

            logger.LogTrace("Base initialization of children merger completed");
        }

        protected abstract Expression<Func<TLink, TChild>> ChildFromLinkExpression { get; }

        protected abstract Expression<Func<TParent, ICollection<TLink>>> CollectionExpression { get; }

        private PropertyInfo ChildProperty => lazyChildProperty.Value;

        private PropertyInfo ChildrenCollectionProperty => lazyChildrenCollectionProperty.Value;

        private Func<TLink, TChild> GetChildFromLinkFunc => lazyGetChildFunc.Value;

        private Func<TParent, ICollection<TLink>> GetChildrenCollectionFunc => lazyGetChildrenCollectionFunc.Value;

        private Expression<Func<TParent, IEnumerable<TLink>>> GetChildrenEnumerableExpression => lazyGetChildrenEnumerableExpression.Value;

        private bool IsOneToManyRelationship => lazyIsOneToManyRelationship.Value;

        public async ValueTask MergeChildren(SynchronizingTuple<TParent> tuple)
        {
            logger.LogTrace(
                "MergeChildren started for tuple ({Local}, {Synchronized}, {Remote})",
                tuple.Local?.GetId(),
                tuple.Synchronized?.GetId(),
                tuple.ExternalID);
            if (!await tuple.AnyAsync(HasChildren).ConfigureAwait(false))
                return;

            logger.LogDebug("Tuple has children");
            await tuple.ForEachAsync(LoadChildren).ConfigureAwait(false);
            tuple.ForEach(CreateEmptyChildrenList);
            logger.LogTrace("Children loaded");

            var tuples = TuplesUtils.CreateSynchronizingTuples(
                GetChildrenCollectionFunc(tuple.Local).Select(GetChildFromLinkFunc),
                GetChildrenCollectionFunc(tuple.Synchronized).Select(GetChildFromLinkFunc),
                GetChildrenCollectionFunc(tuple.Remote).Select(GetChildFromLinkFunc),
                DoesNeedInTuple);

            logger.LogDebug("Created {Count} tuples", tuples.Count);
            foreach (var childTuple in tuples)
                await SynchronizeChild(tuple, childTuple).ConfigureAwait(false);
            logger.LogTrace("Children synchronized");
        }

        protected abstract bool DoesNeedInTuple(
            TChild child,
            SynchronizingTuple<TChild> childTuple);

        protected virtual Expression<Func<TChild, bool>> GetNeedToRemoveExpression(TParent parent)
            => defaultNeedToRemoveExpression;

        protected virtual bool UnlinkChild(
            TParent parent,
            TChild child)
        {
            if (HasChild(parent, child))
            {
                var first = GetChildrenCollectionFunc(parent)
                   .First(x => GetChildFromLinkFunc(x) == child);
                GetChildrenCollectionFunc(parent).Remove(first);

                var entry = context.Entry(first);
                if (IsOneToManyRelationship && entry.State == EntityState.Deleted)
                    entry.State = EntityState.Modified;
                return true;
            }

            return false;
        }

        private bool AddChild(TParent parent, TChild child)
        {
            if (!HasChild(parent, child))
            {
                if (!IsOneToManyRelationship)
                {
                    var link = new TLink();
                    ChildProperty.SetValue(link, child);
                    GetChildrenCollectionFunc(parent).Add(link);
                    return true;
                }

                GetChildrenCollectionFunc(parent).Add(child as TLink);
                return true;
            }

            return false;
        }

        private void CreateEmptyChildrenList(TParent x)
        {
            if (GetChildrenCollectionFunc(x) == null)
                ChildrenCollectionProperty.SetValue(x, new List<TLink>());
        }

        private bool HasChild(TParent parent, TChild child)
        {
            return GetChildrenCollectionFunc(parent)
               .Any(
                    x =>
                    {
                        var c = GetChildFromLinkFunc(x);
                        return (c.GetId() != 0 && c.GetId() == child.GetId()) || Equals(c, child);
                    });
        }

        private async ValueTask<bool> HasChildren(TParent parent)
        {
            if (GetChildrenCollectionFunc(parent) == null && parent.GetId() != 0)
            {
                return await context.Set<TParent>()
                   .AsNoTracking()
                   .Where(x => x == parent)
                   .Select(GetChildrenEnumerableExpression)
                   .AnyAsync()
                   .ConfigureAwait(false);
            }

            return (GetChildrenCollectionFunc(parent)?.Count ?? 0) > 0;
        }

        private async ValueTask LoadChildren(TParent parent)
        {
            if (GetChildrenCollectionFunc(parent) == null)
            {
                if (parent.GetId() != 0)
                {
                    var collection = context.Entry(parent)
                       .Collection(GetChildrenEnumerableExpression);

                    if (IsOneToManyRelationship)
                    {
                        await collection.LoadAsync()
                           .ConfigureAwait(false);
                    }
                    else
                    {
                        await collection.Query()
                           .Include(ChildFromLinkExpression)
                           .LoadAsync()
                           .ConfigureAwait(false);
                    }
                }
            }
        }

        private async ValueTask<bool> RemoveChild(
            TParent parent,
            TChild child)
        {
            if (child == null)
                return false;

            var result = UnlinkChild(parent, child);

            if (child.GetId() != 0)
            {
                if (await context.Set<TChild>()
                       .AsNoTracking()
                       .Where(x => x == child)
                       .AnyAsync(GetNeedToRemoveExpression(parent))
                       .ConfigureAwait(false))
                    context.Set<TChild>().Remove(child);
            }

            return result;
        }

        private async ValueTask SynchronizeChild(
            SynchronizingTuple<TParent> tuple,
            SynchronizingTuple<TChild> childTuple)
        {
            var action = childTuple.DetermineAction();

            attacher?.AttachExisting(childTuple);

            if (action is SynchronizingAction.Merge
                or SynchronizingAction.AddToLocal
                or SynchronizingAction.AddToRemote)
                await childMerger.Merge(childTuple).ConfigureAwait(false);

            switch (action)
            {
                case SynchronizingAction.Nothing:
                    break;
                case SynchronizingAction.Merge:
                case SynchronizingAction.AddToLocal:
                case SynchronizingAction.AddToRemote:
                    tuple.ForEachChange(childTuple, AddChild);
                    break;
                case SynchronizingAction.RemoveFromLocal:
                case SynchronizingAction.RemoveFromRemote:
                    await tuple.ForEachChangeAsync(childTuple, RemoveChild).ConfigureAwait(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), "Incorrect action");
            }

            tuple.SynchronizeChanges(childTuple);
        }
    }
}
