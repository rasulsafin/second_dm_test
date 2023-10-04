using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brio.Docs.Database;
using Brio.Docs.Database.Extensions;
using Brio.Docs.Database.Models;
using Brio.Docs.Synchronization.Extensions;
using Brio.Docs.Synchronization.Interfaces;
using Brio.Docs.Synchronization.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Synchronization.Utilities.Finders
{
    public class ObjectiveAttacher : IAttacher<Objective>
    {
        private readonly DMContext context;
        private readonly ILogger<ObjectiveAttacher> logger;

        public ObjectiveAttacher(DMContext context, ILogger<ObjectiveAttacher> logger)
        {
            this.context = context;
            this.logger = logger;

            logger.LogTrace("ObjectiveAttacher created");
        }

        public IReadOnlyCollection<Objective> RemoteCollection { get; set; }

        public async Task AttachExisting(SynchronizingTuple<Objective> tuple)
        {
            var id = tuple.ExternalID;
            var needToAttach = !string.IsNullOrEmpty(id) && tuple.Any(x => x == null);
            logger.LogStartAction(tuple, needToAttach ? LogLevel.Debug : LogLevel.Trace);

            if (!needToAttach)
                return;

            if (tuple.Remote == null)
            {
                var (localProject, syncProject) = (tuple.Local?.ProjectID, tuple.Synchronized?.ProjectID);

                localProject ??= await context.Projects
                   .AsNoTracking()
                   .Where(x => x.SynchronizationMateID == syncProject)
                   .Select(x => x.ID)
                   .FirstOrDefaultAsync()
                   .ConfigureAwait(false);

                syncProject ??= await context.Projects
                   .AsNoTracking()
                   .Where(x => x.ID == localProject)
                   .Select(x => x.SynchronizationMateID)
                   .FirstOrDefaultAsync()
                   .ConfigureAwait(false);

                tuple.Remote = RemoteCollection
                   .Where(x => x.ProjectID == localProject || x.ProjectID == syncProject)
                   .FirstOrDefault(x => x.ExternalID == id);
            }

            if (tuple.Remote != null)
            {
                var (localProject, syncProject) = await SearchingUtilities
                   .GetProjectsByRemote(context, tuple.Remote.ProjectID)
                   .ConfigureAwait(false);

                tuple.Local ??= await context.Objectives
                   .Unsynchronized()
                   .Where(x => x.Project == localProject)
                   .FirstOrDefaultAsync(x => x.ExternalID == id)
                   .ConfigureAwait(false);

                tuple.Synchronized ??= await context.Objectives
                   .Synchronized()
                   .Where(x => x.Project == syncProject)
                   .FirstOrDefaultAsync(x => x.ExternalID == id)
                   .ConfigureAwait(false);
            }

            logger.LogDebug(
                "AttachExisting project ends with tuple ({Local}, {Synchronized}, {Remote})",
                tuple.Local?.ID,
                tuple.Synchronized?.ID,
                tuple.ExternalID);
        }
    }
}
