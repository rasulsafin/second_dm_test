using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Brio.Docs.Database;
using Brio.Docs.Integration.Extensions;
using Brio.Docs.Integration.Interfaces;
using Brio.Docs.Synchronization.Extensions;
using Brio.Docs.Synchronization.Models;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Synchronization.Strategies
{
    internal class StrategyHelper
    {
        private readonly DMContext context;
        private readonly ILogger<StrategyHelper> logger;
        private readonly IMapper mapper;

        public StrategyHelper(
            DMContext context,
            IMapper mapper,
            ILogger<StrategyHelper> logger)
        {
            this.context = context;
            this.mapper = mapper;
            this.logger = logger;
            logger.LogTrace("ASynchronizationStrategy created");
        }

        public Task<SynchronizingResult> AddToLocal<TDB>(
            SynchronizingTuple<TDB> tuple,
            CancellationToken token)
            where TDB : class, ISynchronizable<TDB>, new()
        {
            using var lScope = logger.BeginMethodScope();

            try
            {
                token.ThrowIfCancellationRequested();

                UpdateDB(tuple);
                logger.LogTrace("DB entities added");
                return Task.FromResult<SynchronizingResult>(null);
            }
            catch (OperationCanceledException)
            {
                logger.LogCanceled();
                throw;
            }
            catch (Exception e)
            {
                logger.LogExceptionOnAction(SynchronizingAction.AddToLocal, e, tuple);
                return Task.FromResult(new SynchronizingResult
                {
                    Exception = e,
                    Object = tuple.Remote,
                    ObjectType = ObjectType.Remote,
                });
            }
        }

        public async Task<SynchronizingResult> AddToRemote<TDB, TDto>(
            ISynchronizer<TDto> synchronizer,
            SynchronizingTuple<TDB> tuple,
            CancellationToken token)
            where TDB : class, ISynchronizable<TDB>, new()
        {
            using var lScope = logger.BeginMethodScope();

            try
            {
                token.ThrowIfCancellationRequested();

                await UpdateRemote<TDB, TDto>(tuple, synchronizer.Add).ConfigureAwait(false);
                logger.LogTrace("Added to remote");
                UpdateDB(tuple);
                logger.LogTrace("DB entities updated");
                return null;
            }
            catch (OperationCanceledException)
            {
                logger.LogCanceled();
                throw;
            }
            catch (Exception e)
            {
                logger.LogExceptionOnAction(SynchronizingAction.AddToRemote, e, tuple);
                tuple.Local.SynchronizationMate = null;
                return new SynchronizingResult
                {
                    Exception = e,
                    Object = tuple.Local,
                    ObjectType = ObjectType.Local,
                };
            }
        }

        public async Task<SynchronizingResult> Merge<TDB, TDto>(
            SynchronizingTuple<TDB> tuple,
            ISynchronizer<TDto> synchronizer,
            CancellationToken token)
            where TDB : class, ISynchronizable<TDB>, new()
        {
            using var lScope = logger.BeginMethodScope();

            try
            {
                token.ThrowIfCancellationRequested();

                await UpdateRemote<TDB, TDto>(tuple, synchronizer.Update).ConfigureAwait(false);
                logger.LogTrace("Remote updated");
                UpdateDB(tuple);
                logger.LogTrace("DB entities updated");

                return null;
            }
            catch (OperationCanceledException)
            {
                logger.LogCanceled();
                throw;
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

        public Task<SynchronizingResult> RemoveFromLocal<TDB>(
            SynchronizingTuple<TDB> tuple,
            CancellationToken token)
            where TDB : class, ISynchronizable<TDB>, new()
        {
            using var lScope = logger.BeginMethodScope();

            try
            {
                token.ThrowIfCancellationRequested();

                RemoveFromDB(tuple);
                logger.LogTrace("DB entities removed");
                return Task.FromResult<SynchronizingResult>(null);
            }
            catch (OperationCanceledException)
            {
                logger.LogCanceled();
                throw;
            }
            catch (Exception e)
            {
                logger.LogExceptionOnAction(SynchronizingAction.RemoveFromLocal, e, tuple);
                return Task.FromResult(new SynchronizingResult
                {
                    Exception = e,
                    Object = tuple.Local,
                    ObjectType = ObjectType.Local,
                });
            }
        }

        public async Task<SynchronizingResult> RemoveFromRemote<TDB, TDto>(
            ISynchronizer<TDto> synchronizer,
            SynchronizingTuple<TDB> tuple,
            CancellationToken token)
            where TDB : class, ISynchronizable<TDB>, new()
        {
            using var lScope = logger.BeginMethodScope();

            try
            {
                token.ThrowIfCancellationRequested();

                await RemoveFromRemote<TDB, TDto>(tuple, synchronizer.Remove).ConfigureAwait(false);
                logger.LogTrace("Removed from remote");
                RemoveFromDB(tuple);
                logger.LogTrace("DB entities removed");
                return null;
            }
            catch (OperationCanceledException)
            {
                logger.LogCanceled();
                throw;
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

        private void RemoveFromDB<TDB>(SynchronizingTuple<TDB> tuple)
            where TDB : class, ISynchronizable<TDB>, new()
        {
            logger.LogDebug("RemoveFromDB started with tuple {@Tuple}", tuple);
            if (tuple.Local != null)
            {
                context.Set<TDB>().Remove(tuple.Local);
                logger.LogInformation("Removed {@ID}", tuple.Local.ID);
            }

            if (tuple.Synchronized != null)
            {
                context.Set<TDB>().Remove(tuple.Synchronized);
                logger.LogDebug("Removed {@ID}", tuple.Synchronized.ID);
            }
        }

        private async Task RemoveFromRemote<TDB, TDto>(SynchronizingTuple<TDB> tuple, Func<TDto, Task<TDto>> remoteFunc)
            where TDB : class, ISynchronizable<TDB>, new()
        {
            var dto = mapper.Map<TDto>(tuple.Remote);
            logger.LogDebug("Created dto: {@Dto}", dto);
            await remoteFunc(dto).ConfigureAwait(false);
            logger.LogInformation("Removed {ID}", tuple.ExternalID);
        }

        private void UpdateDB<TDB>(SynchronizingTuple<TDB> tuple)
            where TDB : class, ISynchronizable<TDB>, new()
        {
            var set = context.Set<TDB>();

            if (tuple.Synchronized.ID == 0)
            {
                set.Add(tuple.Synchronized);
                logger.LogDebug("Added {ID} to DB", tuple.Synchronized.ExternalID);
            }
            else if (tuple.SynchronizedChanged)
            {
                set.Update(tuple.Synchronized);
                logger.LogDebug("Updated {ID} ({ExternalID})", tuple.Synchronized.ID, tuple.ExternalID);
            }

            if (tuple.Local.ID == 0)
            {
                set.Add(tuple.Local);
                logger.LogInformation("Added {ID} to local", tuple.Local.ExternalID);
            }
            else if (tuple.LocalChanged)
            {
                set.Update(tuple.Local);
                logger.LogInformation("Updated {ID} ({ExternalID})", tuple.Local.ID, tuple.ExternalID);
            }
        }

        private async Task UpdateRemote<TDB, TDto>(
            SynchronizingTuple<TDB> tuple,
            Func<TDto, Task<TDto>> remoteFunc)
            where TDB : class, ISynchronizable<TDB>, new()
        {
            if (!tuple.RemoteChanged)
                return;

            var result = await remoteFunc(mapper.Map<TDto>(tuple.Remote)).ConfigureAwait(false);
            logger.LogDebug("Remote return {@Data}", result);
            tuple.Remote = mapper.Map<TDB>(result);
            logger.LogInformation("Put {ID} to remote", tuple.ExternalID);
        }
    }
}
