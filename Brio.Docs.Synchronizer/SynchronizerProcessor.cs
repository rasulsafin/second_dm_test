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
using Brio.Docs.Synchronization.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Synchronization
{
    internal class SynchronizerProcessor : ISynchronizerProcessor
    {
        private readonly DMContext context;
        private readonly IAttacher<Objective> objectiveAttacher;
        private readonly ISynchronizationStrategy<Objective> objectiveStrategy;
        private readonly IAttacher<Project> projectAttacher;
        private readonly ISynchronizationStrategy<Project> projectStrategy;
        private readonly ILogger<SynchronizerProcessor> logger;

        public SynchronizerProcessor(
            DMContext context,
            IAttacher<Project> projectAttacher,
            IAttacher<Objective> objectiveAttacher,
            ISynchronizationStrategy<Project> projectStrategy,
            ISynchronizationStrategy<Objective> objectiveStrategy,
            ILogger<SynchronizerProcessor> logger)
        {
            this.context = context;
            this.projectAttacher = projectAttacher;
            this.objectiveAttacher = objectiveAttacher;
            this.projectStrategy = projectStrategy;
            this.objectiveStrategy = objectiveStrategy;
            this.logger = logger;
            logger.LogTrace("ASynchronizationStrategy created");
        }

        public async Task<List<SynchronizingResult>> Synchronize<TDB, TDto>(
            SynchronizingData data,
            IEnumerable<TDB> remoteCollection,
            IQueryable<TDB> set,
            CancellationToken token,
            IProgress<double> progress = null)
            where TDB : class, ISynchronizable<TDB>, new()
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Synchronize started");

            progress?.Report(0.0);

            var dbLocal = await set
               .AsNoTracking()
               .Unsynchronized()
               .Include(x => x.SynchronizationMate)
               .ToListAsync()
               .ConfigureAwait(false);

            var dbSynchronized = await set
               .AsNoTracking()
               .Synchronized()
               .ToListAsync()
               .ConfigureAwait(false);

            var strategy = GetStrategy<TDB>();
            var attacher = GetAttacher<TDB>();

            var local = strategy.Order(dbLocal);
            var synchronized = strategy.Order(dbSynchronized);
            var remote = strategy.Order(remoteCollection);

            var tuples = TuplesUtils.CreateSynchronizingTuples(
                local,
                synchronized,
                remote,
                (element, tuple) => tuple.DoesNeed(element));

            logger.LogDebug("{@Count} tuples created", tuples.Count);

            foreach (var tuple in tuples)
                await attacher.AttachExisting(tuple).ConfigureAwait(false);

            var unloadedTuples = tuples.Select(SynchronizationTupleExtensions.AsUnloaded);

            context.ChangeTracker.Clear();
            var results = new List<SynchronizingResult>();
            var i = 0;

            foreach (var unloaded in unloadedTuples)
            {
                logger.LogTrace(
                    "Tuple ({Local}, {Synchronized}, {Remote})",
                    unloaded.LocalId,
                    unloaded.SynchronizedId,
                    unloaded.Remote?.ExternalID);

                token.ThrowIfCancellationRequested();

                var action = unloaded.DetermineAction();
                var tuple = await context.Load(unloaded).ConfigureAwait(false);
                logger.LogDebug("Tuple {ID} must {@Action}", tuple.ExternalID, action);

                try
                {
                    var func = GetFunction(strategy, action);
                    var synchronizingResult = await func.Invoke(tuple, data, token).ConfigureAwait(false);
                    results.AddIsNotNull(synchronizingResult);

                    await SaveDb(data).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Synchronization failed");

                    var isRemote = action == SynchronizingAction.AddToLocal;
                    results.Add(
                        new SynchronizingResult
                        {
                            Exception = e,
                            Object = isRemote ? tuple.Remote : tuple.Local,
                            ObjectType = isRemote ? ObjectType.Remote : ObjectType.Local,
                        });
                }
                finally
                {
                    context.ChangeTracker.Clear();
                }

                progress?.Report(++i / (double)tuples.Count);
            }

            progress?.Report(1.0);
            return results;
        }

        private static SynchronizationFunc<TDB> GetFunction<TDB>(ISynchronizationStrategy<TDB> strategy, SynchronizingAction action)
            where TDB : class, ISynchronizable<TDB>, new()
        {
            return action switch
            {
                SynchronizingAction.Nothing => (_, _, _) => Task.FromResult(default(SynchronizingResult)),
                SynchronizingAction.Merge => strategy.Merge,
                SynchronizingAction.AddToLocal => strategy.AddToLocal,
                SynchronizingAction.AddToRemote => strategy.AddToRemote,
                SynchronizingAction.RemoveFromLocal => strategy.RemoveFromLocal,
                SynchronizingAction.RemoveFromRemote => strategy.RemoveFromRemote,
                _ => throw new ArgumentOutOfRangeException(nameof(action), "Invalid action")
            };
        }

        private IAttacher<TDB> GetAttacher<TDB>()
        {
            var type = typeof(TDB);
            return type == typeof(Project) ? (IAttacher<TDB>)projectAttacher :
                type == typeof(Objective)  ? (IAttacher<TDB>)objectiveAttacher :
                                             throw new NotSupportedException();
        }

        private ISynchronizationStrategy<TDB> GetStrategy<TDB>()
            where TDB : class
        {
            var type = typeof(TDB);
            return type == typeof(Project) ? (ISynchronizationStrategy<TDB>)projectStrategy :
                type == typeof(Objective)  ? (ISynchronizationStrategy<TDB>)objectiveStrategy :
                                             throw new NotSupportedException();
        }

        private async Task SaveDb(SynchronizingData data)
        {
            if (data.Date == default)
                await context.SaveChangesAsync().ConfigureAwait(false);
            else
                await context.SynchronizationSaveAsync(data.Date).ConfigureAwait(false);
            logger.LogTrace("DB updated");
        }
    }
}
