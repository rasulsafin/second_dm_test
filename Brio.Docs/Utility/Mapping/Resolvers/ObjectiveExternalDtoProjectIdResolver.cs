using System.Linq;
using AutoMapper;
using Brio.Docs.Database;
using Brio.Docs.Database.Extensions;
using Brio.Docs.Database.Models;
using Brio.Docs.Integration.Dtos;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Utility.Mapping.Resolvers
{
    public class ObjectiveExternalDtoProjectIdResolver : IValueResolver<ObjectiveExternalDto, Objective, int>
    {
        private readonly DMContext dbContext;
        private readonly ILogger<ObjectiveExternalDtoProjectIdResolver> logger;

        public ObjectiveExternalDtoProjectIdResolver(DMContext dbContext, ILogger<ObjectiveExternalDtoProjectIdResolver> logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
            logger.LogTrace("ObjectiveExternalDtoProjectIdResolver created");
        }

        public int Resolve(ObjectiveExternalDto source, Objective destination, int destMember, ResolutionContext context)
        {
            logger.LogTrace("Resolve started with source: {@Source} & destination {@Destination}", source, destination);
            var project = dbContext.Projects.Synchronized().FirstOrDefault(x => x.ExternalID == source.ProjectExternalID);
            logger.LogDebug("Found project: {@Project}", project);
            return project?.ID ?? 0;
        }
    }
}
