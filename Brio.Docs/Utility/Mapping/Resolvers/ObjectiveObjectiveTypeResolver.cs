using AutoMapper;
using Brio.Docs.Database;
using Brio.Docs.Database.Models;
using Brio.Docs.Integration.Dtos;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Utility.Mapping.Resolvers
{
    public class ObjectiveObjectiveTypeResolver : IValueResolver<Objective, ObjectiveExternalDto, ObjectiveTypeExternalDto>
    {
        private readonly DMContext dbContext;
        private readonly ILogger<ObjectiveObjectiveTypeResolver> logger;

        public ObjectiveObjectiveTypeResolver(DMContext dbContext, ILogger<ObjectiveObjectiveTypeResolver> logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
            logger.LogTrace("ObjectiveObjectiveTypeResolver created");
        }

        public ObjectiveTypeExternalDto Resolve(Objective source, ObjectiveExternalDto destination, ObjectiveTypeExternalDto destMember, ResolutionContext context)
        {
            logger.LogTrace("Resolve started with source: {@Source} & destination {@Destination}", source, destination);
            var type = dbContext.ObjectiveTypes.Find(source.ObjectiveTypeID);
            logger.LogDebug("Found type: {@Type}", type);
            var objectiveTypeExternal = new ObjectiveTypeExternalDto
            {
                Name = type.Name,
                ExternalId = type.ExternalId,
            };

            return objectiveTypeExternal;
        }
    }
}
