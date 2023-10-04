using System.Linq;
using AutoMapper;
using Brio.Docs.Database;
using Brio.Docs.Database.Models;
using Brio.Docs.Integration.Dtos;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Utility.Mapping.Resolvers
{
    public class ObjectiveExternalDtoAuthorIdResolver : IValueResolver<ObjectiveExternalDto, Objective, int?>
    {
        private readonly DMContext dbContext;
        private readonly ILogger<ObjectiveExternalDtoAuthorIdResolver> logger;

        public ObjectiveExternalDtoAuthorIdResolver(DMContext dbContext, ILogger<ObjectiveExternalDtoAuthorIdResolver> logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
            logger.LogTrace("ObjectiveExternalDtoAuthorIdResolver created");
        }

        public int? Resolve(ObjectiveExternalDto source, Objective destination, int? destMember, ResolutionContext context)
        {
            logger.LogTrace("Resolve started with source: {@Source} & destination {@Destination}", source, destination);
            if (source.AuthorExternalID == null)
                return null;
            var user = dbContext.Users.FirstOrDefault(x => x.ExternalID == source.AuthorExternalID);
            logger.LogDebug("Found user: {@User}", user);
            return user?.ID;
        }
    }
}
