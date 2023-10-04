using System.Collections.Generic;
using System.Threading.Tasks;
using Brio.Docs.Client.Dtos;

namespace Brio.Docs.Client.Services.ForApi
{
    public interface IObjectiveTypeForApiService
    {
        /// <summary>
        /// Get list of objective types accessible to specific Connection Type.
        /// </summary>
        /// <param name="id">User id.</param>
        /// <returns>Objective Type.</returns>
        /// <exception cref="ANotFoundException">Thrown when objective type not found.</exception>
        /// <exception cref="DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<IEnumerable<ObjectiveTypeDto>> GetObjectiveTypes(ID<UserDto> id);

        /// <summary>
        /// Add new objective type.
        /// </summary>
        /// <param name="typeName">Name.</param>
        /// <returns>Id of created objective type.</returns>
        /// <exception cref="ArgumentValidationException">Thrown when objective type with that name already exists.</exception>
        /// <exception cref="DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<ID<ObjectiveTypeDto>> Add(string typeName);

        /// <summary>
        /// Delete objective type by id.
        /// </summary>
        /// <param name="id">Objective Type's id.</param>
        /// <returns>True, if deletion was successful.</returns>
        /// <exception cref="ANotFoundException">Thrown when objective type not found.</exception>
        /// <exception cref="DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<bool> Remove(ID<ObjectiveTypeDto> id);

        /// <summary>
        /// Get Objective Type by id.
        /// </summary>
        /// <param name="id">Objective Type's id.</param>
        /// <returns>Found type.</returns>
        /// <exception cref="ANotFoundException">Thrown when objective type not found.</exception>
        /// <exception cref="DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<ObjectiveTypeDto> Find(ID<ObjectiveTypeDto> id);

        /// <summary>
        /// Get Objective Type by name.
        /// </summary>
        /// <param name="typename">Name of type.</param>
        /// <returns>Found type.</returns>
        /// <exception cref="ANotFoundException">Thrown when objective type with that name not found.</exception>
        /// <exception cref="DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<ObjectiveTypeDto> Find(string typename);
    }
}
