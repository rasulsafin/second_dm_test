using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brio.Docs.Client;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Client.Exceptions;
using Brio.Docs.Client.Services;
using Brio.Docs.Common;
using Brio.Docs.Database;
using Brio.Docs.Database.Extensions;
using Brio.Docs.Integration.Extensions;
using Brio.Docs.Utility.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Services
{
    public class BimElementService : IBimElementService
    {
        private readonly DMContext context;
        private readonly ILogger<BimElementService> logger;

        public BimElementService(DMContext context,
            ILogger<BimElementService> logger)
        {
            this.context = context;
            this.logger = logger;
            logger.LogTrace("BimElementService created");
        }

        public async Task<IEnumerable<BimElementStatusDto>> GetBimElementsStatuses(ID<ProjectDto> projectID)
        {
            using var lScope = logger.BeginMethodScope();
            logger.LogTrace("GetObjectivesWithLocation started with projectID: {@ProjectID}", projectID);
            try
            {
                var dbProject = await context.Projects.Unsynchronized()
                    .FindOrThrowAsync(x => x.ID, (int)projectID);
                logger.LogDebug("Found project: {@DBProject}", dbProject);

                var objectivesWithBimElements = await context.Objectives
                                    .AsNoTracking()
                                    .Unsynchronized()
                                    .Where(x => x.ProjectID == dbProject.ID)
                                    .Include(x => x.BimElements)
                                        .ThenInclude(x => x.BimElement)
                                    .Where(x => x.BimElements.Count > 0)
                                    .ToListAsync();

                var result = objectivesWithBimElements
                    .SelectMany(x => x.BimElements.Select(be => (be.BimElement.GlobalID, x.Status)))
                    .GroupBy(
                        x => x.GlobalID,
                        x => x,
                        (key, list) => new BimElementStatusDto
                        {
                            GlobalID = key,
                            Status = GetObjectStatus(list.Select(x => (ObjectiveStatus)x.Status)),
                        })
                    .ToArray();
                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Can't find objectives by project key {ProjectID}", projectID);
                if (ex is ANotFoundException)
                    throw;
                throw new DocumentManagementException(ex.Message, ex.StackTrace);
            }
        }

        private ObjectiveStatus GetObjectStatus(IEnumerable<ObjectiveStatus> statuses)
        {
            var isDone = true;
            var isReady = true;
            var isInProgress = false;

            foreach (var status in statuses)
            {
                isDone &= status == ObjectiveStatus.Closed;
                isReady &= status == ObjectiveStatus.Closed || status == ObjectiveStatus.Ready;
                isInProgress |= status == ObjectiveStatus.Closed || status == ObjectiveStatus.Ready || status == ObjectiveStatus.InProgress;
            }

            return isDone ? ObjectiveStatus.Closed : isReady ? ObjectiveStatus.Ready : isInProgress ? ObjectiveStatus.InProgress : ObjectiveStatus.Open;
        }
    }
}
