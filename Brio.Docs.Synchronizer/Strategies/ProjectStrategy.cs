using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Brio.Docs.Database;
using Brio.Docs.Database.Models;
using Brio.Docs.Integration.Extensions;
using Brio.Docs.Synchronization.Extensions;
using Brio.Docs.Synchronization.Interfaces;
using Brio.Docs.Synchronization.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Synchronization.Strategies
{
    internal class ProjectStrategy : ISynchronizationStrategy<Project>
    {
        private readonly DMContext context;
        private readonly IExternalIdUpdater<Item> itemIdUpdater;
        private readonly ILogger<ProjectStrategy> logger;
        private readonly IMerger<Project> merger;
        private readonly StrategyHelper strategyHelper;

        public ProjectStrategy(
            DMContext context,
            IMerger<Project> merger,
            IExternalIdUpdater<Item> itemIdUpdater,
            ILogger<ProjectStrategy> logger,
            StrategyHelper strategyHelper)
        {
            this.context = context;
            this.merger = merger;
            this.itemIdUpdater = itemIdUpdater;
            this.logger = logger;
            this.strategyHelper = strategyHelper;
            logger.LogTrace("ProjectStrategy created");
        }

        public DbSet<Project> GetDBSet(DMContext source)
            => source.Projects;

        public async Task<SynchronizingResult> AddToLocal(
            SynchronizingTuple<Project> tuple,
            SynchronizingData data,
            CancellationToken token)
        {
            using var lScope = logger.BeginMethodScope();

            try
            {
                await merger.Merge(tuple).ConfigureAwait(false);
                await AddUser(tuple, data).ConfigureAwait(false);

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
                    Object = tuple.Local,
                    ObjectType = ObjectType.Local,
                };
            }
        }

        public async Task<SynchronizingResult> AddToRemote(
            SynchronizingTuple<Project> tuple,
            SynchronizingData data,
            CancellationToken token)
        {
            using var lScope = logger.BeginMethodScope();

            try
            {
                await merger.Merge(tuple).ConfigureAwait(false);
                await AddUser(tuple, data).ConfigureAwait(false);
                var result = await strategyHelper.AddToRemote(data.ConnectionContext.ProjectsSynchronizer, tuple, token)
                   .ConfigureAwait(false);
                UpdateChildrenAfterSynchronization(tuple);
                logger.LogTrace("Children updated");
                await merger.Merge(tuple).ConfigureAwait(false);
                return result;
            }
            catch (Exception e)
            {
                logger.LogExceptionOnAction(SynchronizingAction.AddToRemote, e, tuple);
                return new SynchronizingResult
                {
                    Exception = e,
                    Object = tuple.Remote,
                    ObjectType = ObjectType.Remote,
                };
            }
        }

        public async Task<SynchronizingResult> Merge(
            SynchronizingTuple<Project> tuple,
            SynchronizingData data,
            CancellationToken token)
        {
            using var lScope = logger.BeginMethodScope();

            try
            {
                await merger.Merge(tuple).ConfigureAwait(false);

                await AddUser(tuple, data).ConfigureAwait(false);
                logger.LogTrace("User linked");

                var result = await strategyHelper.Merge(tuple, data.ConnectionContext.ProjectsSynchronizer, token)
                   .ConfigureAwait(false);
                UpdateChildrenAfterSynchronization(tuple);
                logger.LogTrace("Children updated");
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

        public IEnumerable<Project> Order(IEnumerable<Project> project)
            => project;

        public async Task<SynchronizingResult> RemoveFromLocal(
            SynchronizingTuple<Project> tuple,
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
            SynchronizingTuple<Project> tuple,
            SynchronizingData data,
            CancellationToken token)
        {
            using var lScope = logger.BeginMethodScope();

            try
            {
                foreach (var item in tuple.Remote.Items ?? Enumerable.Empty<Item>())
                    item.ProjectID = tuple.Synchronized.ID;

                return await strategyHelper.RemoveFromRemote(data.ConnectionContext.ProjectsSynchronizer, tuple, token)
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

        private async ValueTask AddUser(SynchronizingTuple<Project> tuple, SynchronizingData data)
        {
            logger.LogTrace("SynchronizeItems started with tuple: {@Tuple}, data: {@Data}", tuple, data);

            async ValueTask AddUserLocal(Project project)
            {
                var hasUser = await context.UserProjects
                   .AsNoTracking()
                   .AnyAsync(x => x.Project == project && x.UserID == data.UserId)
                   .ConfigureAwait(false);

                if (!hasUser)
                {
                    await context.Set<UserProject>()
                       .AddAsync(
                            new UserProject
                            {
                                Project = project,
                                UserID = data.UserId,
                            })
                       .ConfigureAwait(false);

                    logger.LogDebug("Added user {ID} to project: {@Project}", data.UserId, project);
                }
            }

            await AddUserLocal(tuple.Local).ConfigureAwait(false);
            logger.LogTrace("Added user to local");
            await AddUserLocal(tuple.Synchronized).ConfigureAwait(false);
            logger.LogTrace("Added user to synchronized");
        }

        private void UpdateChildrenAfterSynchronization(SynchronizingTuple<Project> tuple)
        {
            logger.LogTrace("UpdateChildrenAfterSynchronization started with tuple: {@Tuple}", tuple);
            itemIdUpdater.UpdateExternalIds(
                (tuple.Local.Items ?? ArraySegment<Item>.Empty).Concat(
                    tuple.Synchronized.Items ?? ArraySegment<Item>.Empty),
                tuple.Remote.Items ?? ArraySegment<Item>.Empty);
        }
    }
}
