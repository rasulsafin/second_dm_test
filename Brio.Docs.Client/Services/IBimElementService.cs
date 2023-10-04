using System.Collections.Generic;
using System.Threading.Tasks;
using Brio.Docs.Client.Dtos;

namespace Brio.Docs.Client.Services
{
    /// <summary>
    /// Service for bim elements.
    /// </summary>
    public interface IBimElementService
    {
        /// <summary>
       /// Returns list of bim elements with computed statuses.
       /// </summary>
       /// <param name="projectID">Project's ID.</param>
       /// <returns>Collection of objectives.</returns>
       /// <exception cref="ANotFoundException">Thrown when project does not exist.</exception>
       /// <exception cref="DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<IEnumerable<BimElementStatusDto>> GetBimElementsStatuses(ID<ProjectDto> projectID);
    }
}
