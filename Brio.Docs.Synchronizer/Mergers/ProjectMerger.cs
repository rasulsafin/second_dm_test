using System;
using System.Threading.Tasks;
using Brio.Docs.Database.Models;
using Brio.Docs.Integration.Factories;
using Brio.Docs.Synchronization.Extensions;
using Brio.Docs.Synchronization.Interfaces;
using Brio.Docs.Synchronization.Models;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Synchronization.Mergers
{
    internal class ProjectMerger : IMerger<Project>
    {
        private readonly ILogger<ProjectMerger> logger;
        private readonly Lazy<IChildrenMerger<Project, Item>> itemChildrenMerger;

        public ProjectMerger(IFactory<IChildrenMerger<Project, Item>> itemChildrenMergerFactory, ILogger<ProjectMerger> logger)
        {
            this.logger = logger;

            this.itemChildrenMerger = new Lazy<IChildrenMerger<Project, Item>>(itemChildrenMergerFactory.Create);
            logger.LogTrace("ProjectMerger created");
        }

        public async ValueTask Merge(SynchronizingTuple<Project> tuple)
        {
            logger.LogTrace(
                "Merge project started for tuple ({Local}, {Synchronized}, {Remote})",
                tuple.Local?.ID,
                tuple.Synchronized?.ID,
                tuple.ExternalID);
            tuple.Merge(project => project.Title);
            logger.LogAfterMerge(tuple);

            if (tuple.Remote is { Items: { } })
            {
                foreach (var item in tuple.Remote.Items)
                    item.ProjectID = tuple.Synchronized?.ID;
                logger.LogDebug("Project ID set for all remote items");
            }

            await itemChildrenMerger.Value.MergeChildren(tuple).ConfigureAwait(false);
            logger.LogTrace("Project items merged");
        }
    }
}
