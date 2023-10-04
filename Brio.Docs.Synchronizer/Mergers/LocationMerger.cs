using System;
using System.Linq;
using System.Threading.Tasks;
using Brio.Docs.Database;
using Brio.Docs.Database.Models;
using Brio.Docs.Synchronization.Extensions;
using Brio.Docs.Synchronization.Interfaces;
using Brio.Docs.Synchronization.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Synchronization.Mergers
{
    internal class LocationMerger : IMerger<Location>
    {
        private readonly DMContext context;
        private readonly IAttacher<Item> itemAttacher;
        private readonly IMerger<Item> itemMerger;
        private readonly ILogger<LocationMerger> logger;

        public LocationMerger(
            DMContext context,
            ILogger<LocationMerger> logger,
            IAttacher<Item> itemAttacher,
            IMerger<Item> itemMerger)
        {
            this.context = context;
            this.logger = logger;
            this.itemAttacher = itemAttacher;
            this.itemMerger = itemMerger;
            logger.LogTrace("LocationMerger created");
        }

        public async ValueTask Merge(SynchronizingTuple<Location> tuple)
        {
            logger.LogTrace(
                "Merge location started for tuple ({Local}, {Synchronized}, {Remote})",
                tuple.Local?.ID,
                tuple.Synchronized?.ID,
                tuple.ExternalID);
            tuple.Merge(
                await GetUpdatedTime(tuple.Local).ConfigureAwait(false),
                await GetUpdatedTime(tuple.Remote).ConfigureAwait(false),
                location => location.PositionX,
                location => location.PositionY,
                location => location.PositionZ,
                location => location.CameraPositionX,
                location => location.CameraPositionY,
                location => location.CameraPositionZ,
                location => location.Guid);

            logger.LogAfterMerge(tuple);
            await LinkLocationItem(tuple).ConfigureAwait(false);
            logger.LogTrace("Location item merged");
        }

        private async ValueTask<DateTime> GetUpdatedTime(Location location)
        {
            if (location == null)
                return default;

            if (location.Objective != null)
                return location.Objective.UpdatedAt;

            if (location.ID == 0)
                return default;

            return await context.Set<Location>()
               .AsNoTracking()
               .Where(x => x.ID == location.ID)
               .Select(x => x.Objective.UpdatedAt)
               .FirstOrDefaultAsync()
               .ConfigureAwait(false);
        }

        private async Task LinkLocationItem(SynchronizingTuple<Location> tuple)
        {
            logger.LogTrace("LinkLocationItem started with {@Tuple}", tuple);
            await tuple.ForEachAsync(LoadItem).ConfigureAwait(false);

            var itemTuple = new SynchronizingTuple<Item>(
                local: tuple.Local.Item,
                synchronized: tuple.Synchronized.Item,
                remote: tuple.Remote.Item);

            var idsTuple = new SynchronizingTuple<string>(
                local: itemTuple.Local?.ExternalID,
                synchronized: itemTuple.Synchronized?.ExternalID,
                remote: itemTuple.Remote?.ExternalID);

            var action = idsTuple.DetermineAction();

            if (action == SynchronizingAction.Merge)
            {
                var relevantId = idsTuple.GetRelevant(
                    await GetUpdatedTime(tuple.Local).ConfigureAwait(false),
                    await GetUpdatedTime(tuple.Remote).ConfigureAwait(false));

                bool Remove(ref Item item)
                {
                    if (item?.ExternalID != relevantId)
                        item = null;

                    return false;
                }

                itemTuple.ForEachChange(Remove);
                itemTuple.ExternalID = relevantId;
            }

            await itemAttacher.AttachExisting(itemTuple).ConfigureAwait(false);
            itemTuple.Synchronized ??= itemTuple.Local?.SynchronizationMate;
            await itemMerger.Merge(itemTuple).ConfigureAwait(false);
            tuple.SynchronizeChanges(itemTuple);

            tuple.ForEachChange(
                itemTuple,
                (location, item) =>
                {
                    if (location.Item != item)
                    {
                        location.Item = item;
                        return true;
                    }

                    return false;
                });
        }

        private async ValueTask LoadItem(Location location)
        {
            if (location.Item != null || location.ID == 0)
                return;

            location.Item = await context.Set<Objective>()
               .Where(x => x.Location == location)
               .Include(x => x.Location)
               .ThenInclude(x => x.Item)
               .Select(x => x.Location)
               .Select(x => x.Item)
               .FirstOrDefaultAsync()
               .ConfigureAwait(false);
        }
    }
}
