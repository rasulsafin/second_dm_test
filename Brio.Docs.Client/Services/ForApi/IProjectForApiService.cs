using System.Collections.Generic;
using System.Threading.Tasks;
using Brio.Docs.Client.Dtos;

namespace Brio.Docs.Client.Services.ForApi
{
    /// <summary>
    /// Service for managing Project entities for Api.
    /// </summary>
    public interface IProjectForApiService
    {
        /// <summary>
        /// Get list of all projects.
        /// </summary>
        /// <returns>List of all projects.</returns>
        /// <exception cref="DocumentManagementException">Thrown when something went wrong.</exception>
        Task<IEnumerable<ProjectToListDto>> GetAllProjects();

        /// <summary>
        /// Get projects linked to the specific user.
        /// </summary>
        /// <param name="userID">User's id.</param>
        /// <returns>List of projects.</returns>
        /// <exception cref="ANotFoundException">Thrown when user not found.</exception>
        /// <exception cref="DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<IEnumerable<ProjectToListDto>> GetUserProjects(ID<UserDto> userID);

        /// <summary>
        /// Create new project.
        /// </summary>
        /// <param name="projectToCreate">Project data.</param>
        /// <returns>Created project.</returns>
        /// <exception cref="ArgumentValidationException">Thrown when project data is invalid.</exception>
        /// <exception cref="DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<ProjectToListDto> Add(ProjectToCreateDto projectToCreate);

        /// <summary>
        /// Delete project by its id.
        /// </summary>
        /// <param name="projectID">Project's id.</param>
        /// <returns>True if project was deleted.</returns>
        /// <exception cref="ANotFoundException">Thrown when project not found.</exception>
        /// <exception cref="DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<bool> Remove(ID<ProjectDto> projectID);

        /// <summary>
        /// Update project's values.
        /// </summary>
        /// <param name="project">Project data to update.</param>
        /// <returns>True, if updated successfully.</returns>
        /// <exception cref="ArgumentValidationException">Thrown when project data is invalid.</exception>
        /// <exception cref="ANotFoundException">Thrown when project not found.</exception>
        /// <exception cref="DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<bool> Update(ProjectDto project);

        /// <summary>
        /// Get project by id.
        /// </summary>
        /// <param name="projectID">Project's id.</param>
        /// <returns>Found project.</returns>
        /// <exception cref="ANotFoundException">Thrown when project not found.</exception>
        /// <exception cref="DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<ProjectDto> Find(ID<ProjectDto> projectID);

        /// <summary>
        /// Get list of users that have access to this project.
        /// </summary>
        /// <param name="projectID">Project's id.</param>
        /// <returns>List of users.</returns>
        /// <exception cref="ANotFoundException">Thrown when project not found.</exception>
        /// <exception cref="DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<IEnumerable<UserDto>> GetUsers(ID<ProjectDto> projectID);

        /// <summary>
        /// Link existing project to users.
        /// </summary>
        /// <param name="projectID">Project's id.</param>
        /// <param name="users">List of user's ids connect project to.</param>
        /// <returns>True if linked successfully.</returns>
        /// <exception cref="ANotFoundException">Thrown when project OR user not found.</exception>
        /// <exception cref="DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<bool> LinkToUsers(ID<ProjectDto> projectID, IEnumerable<ID<UserDto>> users);

        /// <summary>
        /// Unlink existing project from list of users.
        /// </summary>
        /// <param name="projectID">Project's id.</param>
        /// <param name="users">List of user's ids unlink project from.</param>
        /// <returns>True if unlinked successfully.</returns>
        /// <exception cref="ANotFoundException">Thrown when project OR user not found.</exception>
        /// <exception cref="DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<bool> UnlinkFromUsers(ID<ProjectDto> projectID, IEnumerable<ID<UserDto>> users);
    }
}
