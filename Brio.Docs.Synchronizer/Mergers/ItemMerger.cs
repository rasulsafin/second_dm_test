using System.Linq;
using System.Threading.Tasks;
using Brio.Docs.Database.Models;
using Brio.Docs.Synchronization.Extensions;
using Brio.Docs.Synchronization.Interfaces;
using Brio.Docs.Synchronization.Models;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Synchronization.Mergers
{
    internal class ItemMerger : IMerger<Item>
    {
        private readonly ILogger<ItemMerger> logger;

        public ItemMerger(ILogger<ItemMerger> logger)
        {
            this.logger = logger;
            logger.LogTrace("ItemMerger created");
        }

        public ValueTask Merge(SynchronizingTuple<Item> tuple)
        {
            logger.LogTrace(
                "Merge item started for tuple ({Local}, {Synchronized}, {Remote})",
                tuple.Local?.ID,
                tuple.Synchronized?.ID,
                tuple.ExternalID);
            tuple.Merge(
                item => item.Name,
                item => item.RelativePath,
                item => item.ItemType);
            if (tuple.Remote is { ProjectID: null, Project: null, Objectives: null })
                tuple.Remote.ProjectID = tuple.AsEnumerable().Select(x => x?.ProjectID).FirstOrDefault(x => x != null);

            logger.LogAfterMerge(tuple);
            return ValueTask.CompletedTask;
        }
    }
}
