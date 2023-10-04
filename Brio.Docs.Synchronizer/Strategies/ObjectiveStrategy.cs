using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Brio.Docs.Database;
using Brio.Docs.Database.Extensions;
using Brio.Docs.Database.Models;
using Brio.Docs.Integration.Extensions;
using Brio.Docs.Synchronization.Extensions;
using Brio.Docs.Synchronization.Interfaces;
using Brio.Docs.Synchronization.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Synchronization.Strategies
{
    internal class ObjectiveStrategy : ISynchronizationStrategy<Objective>
    {
        private readonly DMContext context;
        private readonly IExternalIdUpdater<DynamicField> dynamicFieldIdUpdater;
        private readonly IExternalIdUpdater<Item> itemIdUpdater;
        private readonly ILogger<ObjectiveStrategy> logger;
        private readonly IMerger<Objective> merger;
        private readonly StrategyHelper strategyHelper;

        public ObjectiveStrategy(
            StrategyHelper strategyHelper,
            DMContext context,
            IMerger<Objective> merger,
            IExternalIdUpdater<DynamicField> dynamicFieldIdUpdater,
            IExternalIdUpdater<Item> itemIdUpdater,
            ILogger<ObjectiveStrategy> logger)
        {
            this.strategyHelper = strategyHelper;
            this.context = context;
            this.merger = merger;
            this.dynamicFieldIdUpdater = dynamicFieldIdUpdater;
            this.itemIdUpdater = itemIdUpdater;
            this.logger = logger;
            logger.LogTrace("ObjectiveStrategy created");
        }

        public DbSet<Objective> GetDBSet(DMContext source)
            => source.Objectives;

        public async Task<SynchronizingResult> AddToLocal(
            SynchronizingTuple<Objective> tuple,
            SynchronizingData data,
            CancellationToken token)
        {
            using var lScope = logger.BeginMethodScope();

            try
            {
                await merger.Merge(tuple).ConfigureAwait(false);

                await UpdateChildrenBeforeSynchronization(tuple, data).ConfigureAwait(false);
                await CreateObjectiveParentLink(tuple).ConfigureAwait(false);
                tuple.Synchronized.ProjectID = tuple.Remote.ProjectID;
                var id = tuple.Synchronized.ProjectID;
                tuple.Local.ProjectID = await context.Projects
                   .AsNoTracking()
                   .Where(x => x.SynchronizationMateID == id)
                   .Select(x => x.ID)
                   .FirstOrDefaultAsync(CancellationToken.None)
                   .ConfigureAwait(false);

                var resultAfterBase = await strategyHelper.AddToLocal(tuple, token).ConfigureAwait(false);
                if (resultAfterBase != null)
                    throw resultAfterBase.Exception;

                await merger.Merge(tuple).ConfigureAwait(false);
                return null;
            }
            catch (Exception e)
            {
                logger.LogExceptionOnAction(SynchronizingAction.AddToLocal, e, tuple);
                return new SynchronizingResult
                {
                    Exception = e,
                    Object = tuple.Remote,
                    ObjectType = ObjectType.Remote,
                };
            }
        }

        public async Task<SynchronizingResult> AddToRemote(
            SynchronizingTuple<Objective> tuple,
            SynchronizingData data,
            CancellationToken token)
        {
            using var lScope = logger.BeginMethodScope();

            try
            {
                await merger.Merge(tuple).ConfigureAwait(false);

                await UpdateChildrenBeforeSynchronization(tuple, data).ConfigureAwait(false);
                await CreateObjectiveParentLink(tuple).ConfigureAwait(false);
                var projectID = tuple.Local.ProjectID;
                var projectMateId = await context.Projects
                   .AsNoTracking()
                   .Where(x => x.ID == projectID)
                   .Where(x => x.SynchronizationMateID != null)
                   .Select(x => x.SynchronizationMateID)
                   .FirstOrDefaultAsync(CancellationToken.None)
                   .ConfigureAwait(false);
                tuple.Remote.ProjectID = tuple.Synchronized.ProjectID = projectMateId ?? 0;

                var result = await strategyHelper
                   .AddToRemote(data.ConnectionContext.ObjectivesSynchronizer, tuple, token)
                   .ConfigureAwait(false);
                await UpdateChildrenAfterSynchronization(tuple, data).ConfigureAwait(false);
                await merger.Merge(tuple).ConfigureAwait(false);
                return result;
            }
            catch (Exception e)
            {
                logger.LogExceptionOnAction(SynchronizingAction.AddToRemote, e, tuple);
                return new SynchronizingResult
                {
                    Exception = e,
                    Object = tuple.Local,
                    ObjectType = ObjectType.Local,
                };
            }
        }

        public async Task<SynchronizingResult> Merge(
            SynchronizingTuple<Objective> tuple,
            SynchronizingData data,
            CancellationToken token)
        {
            using var lScope = logger.BeginMethodScope();

            try
            {
                await merger.Merge(tuple).ConfigureAwait(false);
                await UpdateChildrenBeforeSynchronization(tuple, data).ConfigureAwait(false);
                var result = await strategyHelper.Merge(tuple, data.ConnectionContext.ObjectivesSynchronizer, token)
                   .ConfigureAwait(false);
                await UpdateChildrenAfterSynchronization(tuple, data).ConfigureAwait(false);
                await merger.Merge(tuple).ConfigureAwait(false);
                return result;
            }
            catch (Exception e)
            {
                logger.LogExceptionOnAction(SynchronizingAction.Merge, e, tuple);
                return new SynchronizingResult
                {
                    Exception = e,
                    Object = tuple.Local,
                    ObjectType = ObjectType.Local,
                };
            }
        }

        public IEnumerable<Objective> Order(IEnumerable<Objective> enumeration)
            => enumeration.OrderByParent(x => x.ParentObjective);

        public async Task<SynchronizingResult> RemoveFromLocal(
            SynchronizingTuple<Objective> tuple,
            SynchronizingData data,
            CancellationToken token)
        {
            using var lScope = logger.BeginMethodScope();

            try
            {
                return await strategyHelper.RemoveFromLocal(tuple, token).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                logger.LogExceptionOnAction(SynchronizingAction.RemoveFromLocal, e, tuple);
                return new SynchronizingResult
                {
                    Exception = e,
                    Object = tuple.Local,
                    ObjectType = ObjectType.Local,
                };
            }
        }

        public async Task<SynchronizingResult> RemoveFromRemote(
            SynchronizingTuple<Objective> tuple,
            SynchronizingData data,
            CancellationToken token)
        {
            using var lScope = logger.BeginMethodScope();

            try
            {
                await UpdateChildrenBeforeSynchronization(tuple, data).ConfigureAwait(false);
                return await strategyHelper
                   .RemoveFromRemote(data.ConnectionContext.ObjectivesSynchronizer, tuple, token)
                   .ConfigureAwait(false);
            }
            catch (Exception e)
            {
                logger.LogExceptionOnAction(SynchronizingAction.RemoveFromRemote, e, tuple);
                return new SynchronizingResult
                {
                    Exception = e,
                    Object = tuple.Remote,
                    ObjectType = ObjectType.Remote,
                };
            }
        }

        private static void AddProjectToRemote(SynchronizingTuple<Objective> tuple)
        {
            tuple.Remote.Project ??= tuple.Synchronized.Project;
            if (tuple.Remote.ProjectID == 0)
                tuple.Remote.ProjectID = tuple.Synchronized.ProjectID;
        }

        private static void AddProjectToRemoteItems(SynchronizingTuple<Objective> tuple)
        {
            if (tuple.Remote.Items == null)
                return;

            foreach (var link in tuple.Remote.Items)
            {
                var item = link.Item;
                item.ProjectID = tuple.AsEnumerable().Select(x => x?.ProjectID ?? 0).First(x => x != 0);
            }
        }

        private async ValueTask AddConnectionInfoToDynamicFields(SynchronizingTuple<Objective> tuple, SynchronizingData data)
        {
            if (tuple.Remote == null)
                return;

            var connectionInfoId = await context.Users.AsNoTracking()
                .Where(x => x.ID == data.UserId)
                .Select(x => x.ConnectionInfoID)
                .FirstAsync()
                .ConfigureAwait(false);
            var remoteObjective = tuple.Remote;

            foreach (var df in remoteObjective.DynamicFields)
                AddConnectionInfoTo(df);

            void AddConnectionInfoTo(DynamicField df)
            {
                df.ConnectionInfoID ??= connectionInfoId;

                if (df.ChildrenDynamicFields == null)
                    return;

                foreach (var child in df.ChildrenDynamicFields)
                    AddConnectionInfoTo(child);
            }
        }

        private async ValueTask CreateObjectiveParentLink(SynchronizingTuple<Objective> tuple)
        {
            logger.LogTrace("CreateObjectiveParentLink started with {@Tuple}", tuple);
            if (tuple.Local.ParentObjectiveID != null)
            {
                var synchronizedParent = await context.Objectives
                    .Where(x => x.ID == tuple.Local.ParentObjectiveID)
                    .Select(x => x.SynchronizationMate)
                    .FirstOrDefaultAsync()
                    .ConfigureAwait(false);
                tuple.Synchronized.ParentObjective =
                    tuple.Remote.ParentObjective = synchronizedParent;
            }
            else if (tuple.Remote.ParentObjective != null)
            {
                tuple.Synchronized.ParentObjective = await context.Objectives.Synchronized()
                    .FirstAsync(x => string.Equals(x.ExternalID, tuple.Remote.ParentObjective.ExternalID))
                    .ConfigureAwait(false);

                tuple.Local.ParentObjective = await context.Objectives.Unsynchronized()
                    .FirstAsync(x => string.Equals(x.ExternalID, tuple.Remote.ParentObjective.ExternalID))
                    .ConfigureAwait(false);
            }
        }

        private async ValueTask LoadAuthorToRemoteObjectives(SynchronizingTuple<Objective> tuple)
        {
            var remote = tuple.Remote;
            if (remote != null && remote.AuthorID != null && remote.AuthorID != 0 && remote.Author == null)
                remote.Author = await context.Users.FindAsync(remote.AuthorID).ConfigureAwait(false);
        }

        private async ValueTask UpdateChildrenAfterSynchronization(SynchronizingTuple<Objective> tuple, SynchronizingData data)
        {
            logger.LogTrace("UpdateChildrenAfterSynchronization started with {@Tuple}", tuple);
            itemIdUpdater.UpdateExternalIds(
                (tuple.Local.Items ?? ArraySegment<ObjectiveItem>.Empty)
               .Concat(tuple.Synchronized.Items ?? ArraySegment<ObjectiveItem>.Empty)
               .Select(x => x.Item),
                (tuple.Remote.Items ?? ArraySegment<ObjectiveItem>.Empty).Select(x => x.Item).ToArray());
            logger.LogTrace("External ids of items updated");
            dynamicFieldIdUpdater.UpdateExternalIds(
                (tuple.Local.DynamicFields ?? ArraySegment<DynamicField>.Empty)
               .Concat(tuple.Synchronized.DynamicFields ?? ArraySegment<DynamicField>.Empty),
                tuple.Remote.DynamicFields ?? ArraySegment<DynamicField>.Empty);
            logger.LogTrace("External ids of dynamic fields updated");

            await AddConnectionInfoToDynamicFields(tuple, data).ConfigureAwait(false);
            AddProjectToRemote(tuple);
        }

        private async ValueTask UpdateChildrenBeforeSynchronization(SynchronizingTuple<Objective> tuple, SynchronizingData data)
        {
            await AddConnectionInfoToDynamicFields(tuple, data).ConfigureAwait(false);
            AddProjectToRemoteItems(tuple);
            await LoadAuthorToRemoteObjectives(tuple).ConfigureAwait(false);
        }
    }
}
