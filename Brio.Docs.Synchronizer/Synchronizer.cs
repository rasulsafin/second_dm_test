using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Brio.Docs.Database;
using Brio.Docs.Database.Models;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Extensions;
using Brio.Docs.Integration.Interfaces;
using Brio.Docs.Synchronization.Extensions;
using Brio.Docs.Synchronization.Interfaces;
using Brio.Docs.Synchronization.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Synchronization
{
    internal class Synchronizer : ISynchronizer
    {
        private readonly DMContext dbContext;
        private readonly ILogger<Synchronizer> logger;
        private readonly IMapper mapper;
        private readonly IConverter<IReadOnlyCollection<ObjectiveExternalDto>, IReadOnlyCollection<Objective>>
            objectivesMapper;

        private readonly ISynchronizerProcessor processor;
        private readonly IAttacher<Project> projectAttacher;
        private readonly IAttacher<Objective> objectiveAttacher;
        private readonly IConverter<IReadOnlyCollection<ProjectExternalDto>, IReadOnlyCollection<Project>>
            projectsMapper;

        public Synchronizer(
            DMContext dbContext,
            IMapper mapper,
            ISynchronizerProcessor processor,
            IAttacher<Project> projectAttacher,
            IAttacher<Objective> objectiveAttacher,
            ILogger<Synchronizer> logger,
            IConverter<IReadOnlyCollection<ProjectExternalDto>, IReadOnlyCollection<Project>> projectsMapper,
            IConverter<IReadOnlyCollection<ObjectiveExternalDto>, IReadOnlyCollection<Objective>> objectivesMapper)
        {
            this.dbContext = dbContext;
            this.mapper = mapper;
            this.processor = processor;
            this.projectAttacher = projectAttacher;
            this.objectiveAttacher = objectiveAttacher;
            this.logger = logger;
            this.objectivesMapper = objectivesMapper;
            this.projectsMapper = projectsMapper;
        }

        public async Task<ICollection<SynchronizingResult>> Synchronize(
            SynchronizingData data,
            IConnection connection,
            ConnectionInfoExternalDto info,
            IProgress<double> progress,
            CancellationToken token)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("Synchronization started with {@Data}", data);

            var results = new List<SynchronizingResult>();
            var projectProgress = new Progress<double>(v => { progress.Report(v / 2); });
            var objectiveProgress = new Progress<double>(v => { progress.Report((v + 1) / 2); });
            IConnectionContext context = null;

            try
            {
                data.Date = DateTime.UtcNow;
                var userID = data.UserId;
                var lastSynchronization = await GetLastSynchronizationDate(userID).ConfigureAwait(false);
                var externalDto = mapper.Map<ConnectionInfoExternalDto>(info);
                context = await connection.GetContext(externalDto).ConfigureAwait(false);
                data.ConnectionContext = context;

                (int[] unsyncProjectsIDs, string[] unsyncProjectsExternalIDs) =
                    await SynchronizeProjects(data, token, lastSynchronization, results, projectProgress)
                       .ConfigureAwait(false);

                token.ThrowIfCancellationRequested();

                await SynchronizeObjectives(
                        data,
                        token,
                        lastSynchronization,
                        results,
                        unsyncProjectsIDs,
                        unsyncProjectsExternalIDs,
                        objectiveProgress)
                   .ConfigureAwait(false);

                token.ThrowIfCancellationRequested();

                await dbContext.Synchronizations.AddAsync(CreateNewSynchronizationData(data), CancellationToken.None)
                   .ConfigureAwait(false);

                logger.LogTrace("Added synchronization date");
                await dbContext.SynchronizationSaveAsync(data.Date, CancellationToken.None).ConfigureAwait(false);
                logger.LogDebug("DB updated");
                await dbContext.SynchronizationSaveAsync(data.Date, CancellationToken.None).ConfigureAwait(false);
                logger.LogDebug("DB updated");
            }
            catch (OperationCanceledException)
            {
                logger.LogCanceled();
                return results;
            }
            catch (Exception e)
            {
                logger.LogCritical(e, "Synchronization failed");
                results.Add(new SynchronizingResult { Exception = e });
                progress?.Report(1.0);
                return results;
            }
            finally
            {
                if (context is IDisposable disposable)
                    disposable.Dispose();
                logger.LogTrace("Context closed");
            }

            return results;
        }

        private static Database.Models.Synchronization CreateNewSynchronizationData(SynchronizingData data)
        {
            return new Database.Models.Synchronization
            {
                Date = data.Date,
                UserID = data.UserId,
            };
        }

        private IQueryable<T> FilterNewAndUpdated<T>(
            IQueryable<T> collection,
            Expression<Func<T, bool>> defaultFilter,
            string[] updatedIds)
            where T : class, ISynchronizableBase
            => collection.Where(defaultFilter).Where(x => x.ExternalID == null || updatedIds.Contains(x.ExternalID));

        private async Task<DateTime> GetLastSynchronizationDate(int userID)
        {
            return (await dbContext.Synchronizations.Where(x => x.UserID == userID)
                   .OrderBy(x => x.Date)
                   .LastOrDefaultAsync(CancellationToken.None)
                   .ConfigureAwait(false))?.Date ??
                DateTime.MinValue;
        }

        private async Task<string[]> GetUpdatedIDs<TDB, TDto>(
            DateTime date,
            IQueryable<TDB> set,
            ISynchronizer<TDto> synchronizer)
            where TDB : class, ISynchronizable<TDB>
        {
            logger.LogTrace("GetUpdatedIDs started with date: {@Date}", date);

            // TODO: GetAllIDs to know what is removed from remote.
            var remoteUpdated = (await synchronizer.GetUpdatedIDs(date).ConfigureAwait(false)).ToArray();
            logger.LogDebug("Updated on remote: {@IDs}", (object)remoteUpdated);
            var localUpdated = await set.Where(x => x.UpdatedAt > date)
               .Where(x => x.ExternalID != null)
               .Select(x => x.ExternalID)
               .ToListAsync()
               .ConfigureAwait(false);
            logger.LogDebug("Updated on local: {@IDs}", localUpdated);
            var localRemoved = await set
               .Where(x => x.ExternalID != null)
               .GroupBy(x => x.ExternalID)
               .Where(x => x.Count() < 2)
               .Select(x => x.Key)
               .ToListAsync()
               .ConfigureAwait(false);
            logger.LogDebug("Removed on local: {@IDs}", localRemoved);
            return remoteUpdated.Union(localUpdated).Union(localRemoved).ToArray();
        }

        private async Task SynchronizeObjectives(
            SynchronizingData data,
            CancellationToken token,
            DateTime lastSynchronization,
            List<SynchronizingResult> results,
            int[] unsyncProjectsIDs,
            string[] unsyncProjectsExternalIDs,
            IProgress<double> objectiveProgress)
        {
            var ids = await GetUpdatedIDs(
                    lastSynchronization,
                    dbContext.Objectives.Where(data.ObjectivesFilter),
                    data.ConnectionContext.ObjectivesSynchronizer)
               .ConfigureAwait(false);
            logger.LogDebug("Updated objective ids: {@IDs}", (object)ids);

            var remoteObjectives = await objectivesMapper
               .Convert(await data.ConnectionContext.ObjectivesSynchronizer.Get(ids).ConfigureAwait(false))
               .ConfigureAwait(false);
            objectiveAttacher.RemoteCollection = remoteObjectives;

            var objectives = dbContext.Objectives.Where(
                x =>
                    !unsyncProjectsIDs.Contains(x.ProjectID) &&
                    (x.Project == null || !unsyncProjectsExternalIDs.Contains(x.Project.ExternalID)));

            objectives = FilterNewAndUpdated(objectives, data.ObjectivesFilter, ids);

            var synchronizingResults = await processor.Synchronize<Objective, ObjectiveExternalDto>(
                    data,
                    remoteObjectives.Where(x => data.ObjectivesFilter.Compile().Invoke(x)),
                    objectives,
                    token,
                    objectiveProgress)
               .ConfigureAwait(false);

            results.AddRange(synchronizingResults);
            logger.LogInformation("Objective synchronized");
        }

        private async Task<(int[] unsyncProjectsIDs, string[] unsyncProjectsExternalIDs)> SynchronizeProjects(
            SynchronizingData data,
            CancellationToken token,
            DateTime lastSynchronization,
            List<SynchronizingResult> results,
            IProgress<double> projectProgress)
        {
            var ids = await GetUpdatedIDs(
                    lastSynchronization,
                    dbContext.Projects.Where(data.ProjectsFilter),
                    data.ConnectionContext.ProjectsSynchronizer)
               .ConfigureAwait(false);
            logger.LogDebug("Updated project ids: {@IDs}", (object)ids);
            var remoteProjects = await projectsMapper
               .Convert(await data.ConnectionContext.ProjectsSynchronizer.Get(ids).ConfigureAwait(false))
               .ConfigureAwait(false);

            foreach (var project in remoteProjects)
            {
                project.Users = new List<UserProject>
                {
                    new ()
                    {
                        UserID = data.UserId,
                        Project = project,
                    },
                };
            }

            var projects = FilterNewAndUpdated(dbContext.Projects, data.ProjectsFilter, ids);
            projectAttacher.RemoteCollection = remoteProjects;

            var synchronizingResults = await processor.Synchronize<Project, ProjectExternalDto>(
                    data,
                    remoteProjects.Where(x => data.ProjectsFilter.Compile().Invoke(x)),
                    projects,
                    token,
                    projectProgress)
               .ConfigureAwait(false);

            results.AddRange(synchronizingResults);
            logger.LogInformation("Projects synchronized");
            var unsyncProjectsIDs = results.Where(x => x.ObjectType == ObjectType.Local)
               .Select(x => x.Object.ID)
               .ToArray();
            logger.LogDebug("Unsynchronized projects: {@IDs}", unsyncProjectsIDs);
            var unsyncProjectsExternalIDs = results.Where(x => x.ObjectType == ObjectType.Remote)
               .Select(x => x.Object.ExternalID)
               .ToArray();
            logger.LogDebug("Unsynchronized projects: {@IDs}", (object)unsyncProjectsExternalIDs);
            return (unsyncProjectsIDs, unsyncProjectsExternalIDs);
        }
    }
}
