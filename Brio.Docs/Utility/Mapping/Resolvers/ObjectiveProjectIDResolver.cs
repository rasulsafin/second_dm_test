using AutoMapper;
using Brio.Docs.Database;
using Brio.Docs.Database.Models;
using Brio.Docs.Integration.Dtos;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Utility.Mapping.Resolvers
{
    public class ObjectiveProjectIDResolver : IValueResolver<Objective, ObjectiveExternalDto, string>
    {
        private readonly DMContext dbContext;
        private readonly ILogger<ObjectiveProjectIDResolver> logger;

        public ObjectiveProjectIDResolver(DMContext dbContext, ILogger<ObjectiveProjectIDResolver> logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
            logger.LogTrace("ObjectiveProjectIDResolver created");
        }

        public string Resolve(Objective source, ObjectiveExternalDto destination, string destMember, ResolutionContext context)
        {
            logger.LogTrace("Resolve started with source: {@Source} & destination {@Destination}", source, destination);
            var project = dbContext.Projects.Find(source.ProjectID);
            logger.LogDebug("Found project: {@Project}", project);
            return project.ExternalID;
        }
    }
}
