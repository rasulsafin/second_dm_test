using System.Threading.Tasks;
using Brio.Docs.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Brio.Docs.Database
{
    public class ObjectiveItemContainer : IItemContainer
    {
        private readonly DMContext context;
        private readonly Objective objective;

        public ObjectiveItemContainer(DMContext context, Objective objective)
        {
            this.context = context;
            this.objective = objective;
        }

        public int ItemParentID => objective.ProjectID;

        public async Task<bool> IsItemLinked(Item item)
        {
            return await context.ObjectiveItems
               .AnyAsync(i => i.ItemID == item.ID && i.ObjectiveID == objective.ID);
        }
    }
}
