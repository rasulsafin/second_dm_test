using System.Collections.Generic;
using System.Linq;
using Brio.Docs.Database.Models;
using Brio.Docs.Synchronization.Interfaces;

namespace Brio.Docs.Synchronization.Strategies
{
    internal class ItemExternalIdUpdater : IExternalIdUpdater<Item>
    {
        public void UpdateExternalIds(IEnumerable<Item> local, IEnumerable<Item> remote)
        {
            foreach (var item in local.Where(x => string.IsNullOrWhiteSpace(x.ExternalID)))
                item.ExternalID = remote.FirstOrDefault(x => x.RelativePath == item.RelativePath)?.ExternalID;
        }
    }
}
