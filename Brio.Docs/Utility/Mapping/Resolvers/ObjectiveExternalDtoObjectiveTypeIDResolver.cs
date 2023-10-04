using System.Linq;
using AutoMapper;
using Brio.Docs.Database;
using Brio.Docs.Database.Models;
using Brio.Docs.Integration.Dtos;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Utility.Mapping.Resolvers
{
    public class ObjectiveExternalDtoObjectiveTypeIdResolver : IValueResolver<ObjectiveExternalDto, Objective, int>
    {
        private readonly DMContext dbContext;
        private readonly ILogger<ObjectiveExternalDtoObjectiveTypeIdResolver> logger;

        public ObjectiveExternalDtoObjectiveTypeIdResolver(
            DMContext dbContext,
            ILogger<ObjectiveExternalDtoObjectiveTypeIdResolver> logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
            logger.LogTrace("ObjectiveExternalDtoAuthorResolver created");
        }

        public int Resolve(ObjectiveExternalDto source, Objective destination, int destMember, ResolutionContext context)
        {
            logger.LogTrace("Resolve started with source: {@Source} & destination {@Destination}", source, destination);
            var objectiveTypeID = dbContext.ObjectiveTypes.FirstOrDefault(x => x.ExternalId == source.ObjectiveType.ExternalId).ID;
            logger.LogDebug("Found objectiveTypeID: {@ObjectiveTypeID}", objectiveTypeID);
            return objectiveTypeID;
        }
    }
}
