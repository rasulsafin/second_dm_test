using System;
using System.Linq;
using AutoMapper;
using Brio.Docs.Database;
using Brio.Docs.Database.Models;
using Brio.Docs.Integration.Dtos;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Utility.Mapping.Resolvers
{
    public class ItemProjectDirectoryResolver : IValueResolver<Item, ItemExternalDto, string>
    {
        private readonly DMContext dbContext;
        private readonly ILogger<ItemProjectDirectoryResolver> logger;

        public ItemProjectDirectoryResolver(DMContext dbContext, ILogger<ItemProjectDirectoryResolver> logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
            logger.LogTrace("ItemFullPathResolver created");
        }

        public string Resolve(Item source, ItemExternalDto destination, string destMember, ResolutionContext context)
        {
            logger.LogTrace("Resolve started with source: {@Source} & destination {@Destination}", source, destination);
            var projectID = source.Project?.ID ??
                source.ProjectID ?? source.Objectives?.FirstOrDefault()?.Objective?.ProjectID ?? 0;
            logger.LogDebug("Project ID of the item = {ProjectID}", projectID);
            var project = dbContext.Projects.FirstOrDefault(x => x.ID == projectID);
            logger.LogDebug("Found project {@Project}", project);

            if (project == null)
                throw new InvalidOperationException("Can not get a project from the item");

            return PathHelper.GetDirectory(project);
        }
    }
}
