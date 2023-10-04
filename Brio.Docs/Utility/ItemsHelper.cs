using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Database;
using Brio.Docs.Database.Extensions;
using Brio.Docs.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Utility
{
    public class ItemsHelper
    {
        private readonly DMContext context;
        private readonly IMapper mapper;
        private readonly ILogger<ItemsHelper> logger;

        public ItemsHelper(
            DMContext context,
            IMapper mapper,
            ILogger<ItemsHelper> logger)
        {
            this.context = context;
            this.mapper = mapper;
            this.logger = logger;
            logger.LogTrace("ItemsHelper created");
        }

        internal async Task<Objective> AddItemsAsync(IEnumerable<ItemDto> itemsDto, Objective objective)
        {
            objective.Items = new List<ObjectiveItem>();
            foreach (var item in itemsDto ?? Enumerable.Empty<ItemDto>())
            {
                await LinkItem(item, objective);
            }

            await context.SaveChangesAsync();
            return objective;
        }

        internal async Task<Objective> UpdateItemsAsync(ICollection<ItemDto> itemDtos, Objective objective)
        {
            var objectiveItems = context.ObjectiveItems.Where(i => i.ObjectiveID == objective.ID).ToList();
            var itemsToUnlink = objectiveItems.Where(o => (!itemDtos?.Any(i => (int)i.ID == o.ItemID)) ?? true);
            logger.LogDebug(
                "Objective's ({ID}) item links to remove: {@ItemsToUnlink}",
                objective.ID,
                itemsToUnlink);

            foreach (var itemDto in itemDtos ?? Enumerable.Empty<ItemDto>())
            {
                await LinkItem(itemDto, objective);
            }

            foreach (var itemDto in itemsToUnlink)
            {
                await UnlinkItem(itemDto.ItemID, objective.ID);
            }

            return objective;
        }

        internal async Task<Item> CheckItemToLink<TParent>(ItemDto item, TParent parent)
           where TParent : IItemContainer
        {
            logger.LogTrace("CheckItemToLink started with item: {@Item}, parent: {@Parent}", item, parent);
            var dbItem = await context.Items.Unsynchronized()
                                      .FirstOrDefaultAsync(i => i.ID == (int)item.ID) ??
                         await context.Items.Unsynchronized()
                                      .FirstOrDefaultAsync(i => i.RelativePath == item.RelativePath);
            logger.LogDebug("Found item: {@Item}", dbItem);

            if (await ShouldCreateNewItem(dbItem, parent, context))
            {
                logger.LogDebug("Should create new item");
                dbItem = mapper.Map<Item>(item);
                logger.LogDebug("Mapped item: {@Item}", dbItem);
                await context.Items.AddAsync(dbItem);
                await context.SaveChangesAsync();
                return dbItem;
            }

            bool alreadyLinked = await parent.IsItemLinked(dbItem);

            logger.LogDebug("Already linked: {IsLinked}", alreadyLinked);
            return alreadyLinked ? null : dbItem;
        }

        private async Task LinkItem(ItemDto item, Objective objective)
        {
            logger.LogTrace("LinkItem started for objective {ID} with item: {@Item}", objective.ID, item);
            var dbItem = await CheckItemToLink(item, new ObjectiveItemContainer(context, objective));

            logger.LogDebug("CheckItemToLink returned {@DBItem}", dbItem);
            if (dbItem == null)
                return;

            var link = new ObjectiveItem
            {
                ObjectiveID = objective.ID,
                ItemID = dbItem.ID,
            };

            await context.ObjectiveItems.AddAsync(link);
            await context.SaveChangesAsync();
        }

        private async Task UnlinkItem(int itemID, int objectiveID)
        {
            logger.LogTrace("UnlinkItem started for objective {ID} with item: {ItemID}", objectiveID, itemID);
            var link = await context.ObjectiveItems
                .Where(x => x.ItemID == itemID && x.ObjectiveID == objectiveID)
                .FirstOrDefaultAsync();

            logger.LogDebug("Found link {@Link}", link);
            if (link == null)
                return;

            context.ObjectiveItems.Remove(link);
            await context.SaveChangesAsync();

            return;
        }

        private async Task<bool> ShouldCreateNewItem<TParent>(Item dbItem, TParent parent, DMContext context)
            where TParent : IItemContainer
        {
            logger.LogTrace("ShouldCreateNewItem started with item: {@Item}, parent: {@Parent}", dbItem, parent);

            // Check if item exists
            if (dbItem == null)
                return true;

            int projectID = parent.ItemParentID;

            // Check if same item exists (linked to same project)
            var item = await context.Items
                .Unsynchronized()
                .FirstOrDefaultAsync(x => x.ProjectID == projectID && x.ID == dbItem.ID);
            logger.LogDebug("Found item: {@Item}", item);

            if (item != default)
                return false;

            // Check if same item exists (linked to any objectives in same project)
            item = await context.ObjectiveItems
                .Where(x => x.Objective.ProjectID == projectID)
                .Select(x => x.Item)
                .FirstOrDefaultAsync(x => x.ID == dbItem.ID);
            logger.LogDebug("Found item: {@Item}", item);

            return item == default;
        }
    }
}
