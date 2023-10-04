using System.Collections.Generic;
using System.Threading.Tasks;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Client.Exceptions;

namespace Brio.Docs.Client.Services
{
    /// <summary>
    /// Service for managing files/items.
    /// </summary>
    public interface IItemService
    {
        /// <summary>
        /// Updates item.
        /// </summary>
        /// <param name="item">Data to update.</param>
        /// <returns>True if updated.</returns>
        /// <exception cref="ANotFoundException">Thrown when items does not exist.</exception>
        /// <exception cref="DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<bool> Update(ItemDto item);

        /// <summary>
        /// Finds item in db.
        /// </summary>
        /// <param name="itemID">Id of item to find.</param>
        /// <returns>Found item.</returns>
        /// <exception cref="ANotFoundException">Thrown when item does not exist.</exception>
        /// <exception cref="DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<ItemDto> Find(ID<ItemDto> itemID);

        /// <summary>
        /// Gets list of items that belongs to that project.
        /// </summary>
        /// <param name="projectID">Project's id.</param>
        /// <returns>Collection of items.</returns>
        /// <exception cref="ANotFoundException">Thrown when project does not exist.</exception>
        /// <exception cref="DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<IEnumerable<ItemDto>> GetItems(ID<ProjectDto> projectID);

        /// <summary>
        /// Gets list of items that belongs to that objective.
        /// </summary>
        /// <param name="objectiveID">Objective's id.</param>
        /// <returns>Collection of items.</returns>
        /// <exception cref="ANotFoundException">Thrown when objective does not exist.</exception>
        /// <exception cref="DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<IEnumerable<ItemDto>> GetItems(ID<ObjectiveDto> objectiveID);

        /// <summary>
        /// Links item to the project.
        /// </summary>
        /// <param name="projectId">The project's ID.</param>
        /// <param name="itemDto">The item to link.</param>
        /// <returns>The item ID linked to the project.</returns>
        Task<ID<ItemDto>> LinkItem(ID<ProjectDto> projectId, ItemDto itemDto);

        /// <summary>
        /// Download files from remote connection to local storage.
        /// </summary>
        /// <param name="userID">User's ID.</param>
        /// <param name="itemIds">List of items' id from database.</param>
        /// <returns>Id of the created long request.</returns>
        /// <exception cref="DocumentManagementException">Thrown when something went wrong.</exception>
        Task<RequestID> DownloadItems(ID<UserDto> userID, IEnumerable<ID<ItemDto>> itemIds);

        /// <summary>
        /// Uploads files from the local storage to the remote connection storage.
        /// </summary>
        /// <param name="userID">User's ID.</param>
        /// <param name="itemIds">List of items' id from database.</param>
        /// <returns>The ID of the created long request.</returns>
        /// <exception cref="ANotFoundException">Thrown when users data not found.</exception>
        /// <exception cref="DocumentManagementException">Thrown when something went wrong.</exception>
        Task<RequestID> UploadItems(ID<UserDto> userID, IEnumerable<ID<ItemDto>> itemIds);

        /// <summary>
        /// Delete items from remote connection.
        /// </summary>
        /// <param name="userID">User's ID.</param>
        /// <param name="itemIds">List of items' id from database.</param>
        /// <returns>True if deleted successfully.</returns>
        /// <exception cref="System.NotImplementedException">Thrown while method is not implemented.</exception>
        /// <exception cref="DocumentManagementException">Thrown when something went wrong.</exception>
        Task<RequestID> DeleteItems(ID<UserDto> userID, IEnumerable<ID<ItemDto>> itemIds);
    }
}
