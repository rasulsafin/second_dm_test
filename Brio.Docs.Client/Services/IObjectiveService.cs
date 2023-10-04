using System.Collections.Generic;
using System.Threading.Tasks;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Client.Exceptions;
using Brio.Docs.Client.Filters;
using Brio.Docs.Client.Sorts;

namespace Brio.Docs.Client.Services
{
    /// <summary>
    /// Service for objectives.
    /// </summary>
    public interface IObjectiveService
    {
        /// <summary>
        /// Get new objective and write in to database.
        /// </summary>
        /// <param name="data">Data for new objective.</param>
        /// <returns>Added objective.</returns>
        /// <exception cref="DocumentManagementException">Thrown when something went wrong.</exception>
        Task<ObjectiveToListDto> Add(ObjectiveToCreateDto data);

        /// <summary>
        /// Delete objectives from database by its id.
        /// </summary>
        /// <param name="objectiveID">Objective's ID.</param>
        /// <returns>List of deleted objective's ids.</returns>
        /// <exception cref="ANotFoundException">Thrown when objective does not exist.</exception>
        /// <exception cref="DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<IEnumerable<ID<ObjectiveDto>>> Remove(ID<ObjectiveDto> objectiveID);

        /// <summary>
        /// Update existing objective.
        /// </summary>
        /// <param name="objectiveData">Objective to  update.</param>
        /// <returns>True if updated successfully.</returns>
        /// <exception cref="ANotFoundException">Thrown when objective does not exist.</exception>
        /// <exception cref="DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<bool> Update(ObjectiveDto objectiveData);

        /// <summary>
        /// Find and return objective by id if exists.
        /// </summary>
        /// <param name="objectiveID">Objective's ID.</param>
        /// <returns>Found objective.</returns>
        /// <exception cref="ANotFoundException">Thrown when objective does not exist.</exception>
        /// <exception cref="DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<ObjectiveDto> Find(ID<ObjectiveDto> objectiveID);

        /// <summary>
        /// Return list of objectives, linked to specific project.
        /// </summary>
        /// <param name="projectID">Project's ID.</param>
        /// <param name="filter">Filtration parameters.</param>
        /// <param name="sort">Sorting parameters.</param>
        /// <returns>Collection of objectives.</returns>
        /// <exception cref="ANotFoundException">Thrown when project does not exist.</exception>
        /// <exception cref="DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<PagedListDto<ObjectiveToListDto>> GetObjectives(ID<ProjectDto> projectID, ObjectiveFilterParameters filter, SortParameters sort);

        /// <summary>
        /// Return list of objectives, included only ID and BimElements, linked to specific project.
        /// </summary>
        /// <param name="projectID">Project's ID.</param>
        /// <param name="filter">Filtration parameters.</param>
        /// <returns>Collection of objectives, included only ID and BimElements.</returns>
        /// <exception cref="ANotFoundException">Thrown when project does not exist.</exception>
        /// <exception cref="DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<IEnumerable<ObjectiveToSelectionDto>> GetObjectivesForSelection(ID<ProjectDto> projectID, ObjectiveFilterParameters filter);

        /// <summary>
        /// Return list of objectives with locations, linked to specific project.
        /// </summary>
        /// <param name="projectID">Project's ID.</param>
        /// <param name="itemName">Name of the item in location.</param>
        /// <param name="filter">Filtration parameters.</param>
        /// <returns>Collection of objectives.</returns>
        /// <exception cref="ANotFoundException">Thrown when project does not exist.</exception>
        /// <exception cref="DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<IEnumerable<ObjectiveToLocationDto>> GetObjectivesWithLocation(ID<ProjectDto> projectID, string itemName, ObjectiveFilterParameters filter);

        /// <summary>
        /// Return list of sub-objectives, linked to specific parent objective.
        /// </summary>
        /// <param name="parentID">Parent's ID.</param>
        /// <returns>Collection of sub-objectives.</returns>
        /// <exception cref="ANotFoundException">Thrown when parent does not exist.</exception>
        /// <exception cref="DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<IEnumerable<SubobjectiveDto>> GetObjectivesByParent(ID<ObjectiveDto> parentID);

        /// <summary>
        /// Return list of bim elements parents of objectives linked to projectID.
        /// </summary>
        /// <param name="projectID">Project's ID.</param>
        /// <returns>Collection of bim elements parents.</returns>
        /// <exception cref="ANotFoundException">Thrown when parent does not exist.</exception>
        /// <exception cref="DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<IEnumerable<ObjectiveBimParentDto>> GetParentsOfObjectivesBimElements(ID<ProjectDto> projectID);
    }
}
