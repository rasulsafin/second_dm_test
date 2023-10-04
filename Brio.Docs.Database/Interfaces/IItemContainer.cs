using System.Threading.Tasks;
using Brio.Docs.Database.Models;

namespace Brio.Docs.Database
{
    /// <summary>
    /// Entity with linked items.
    /// </summary>
    public interface IItemContainer
    {
        /// <summary>
        /// ID of the item's parent entity.
        /// </summary>
        int ItemParentID { get; }

        /// <summary>
        /// Checks if item is linked to this entity.
        /// </summary>
        /// <param name="item">Item to check.</param>
        /// <returns>True if item linked to this entity.</returns>
        Task<bool> IsItemLinked(Item item);
    }
}
