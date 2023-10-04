using System.Collections.Generic;
using System.Threading.Tasks;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Common.Dtos;

namespace Brio.Docs.Client.Services.ForApi
{
    public interface IConnectionForApiService
    {
        /// <summary>
        /// Add new ConnectionInfo and link it to User.
        /// </summary>
        /// <param name="connectionInfo">ConnectionInfo to create.</param>
        /// <returns>True if ConnectionInfo was successfully created.</returns>
        /// <exception cref="ANotFoundException">Thrown when user not found.</exception>
        /// <exception cref="DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<ID<ConnectionInfoDto>> Add(ConnectionInfoToCreateDto connectionInfo);

        /// <summary>
        /// Get ConnectionInfo for the specific user.
        /// </summary>
        /// <param name="userID">User's ID.</param>
        /// <returns>ConnectionInfoDto.</returns>
        /// <exception cref="ANotFoundException">Thrown when user not found.</exception>
        /// <exception cref="DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<ConnectionInfoDto> Get(ID<UserDto> userID);

        /// <summary>
        /// Connect user to Remote connection(e.g. YandexDisk, TDMS, BIM360), using user's ConnectionInfo.
        /// </summary>
        /// <param name="userID">User's ID.</param>
        /// <returns>Id of the created long request.</returns>
        /// <exception cref="DocumentManagementException">Thrown when something went wrong.</exception>
        Task<RequestID> Connect(ID<UserDto> userID);

        /// <summary>
        /// Get current status of user's connection.
        /// </summary>
        /// <param name="userID">User's ID.</param>
        /// <returns>Status of the connection.</returns>
        /// <exception cref="ANotFoundException">Thrown when user not found.</exception>
        /// <exception cref="DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<ConnectionStatusDto> GetRemoteConnectionStatus(ID<UserDto> userID);

        /// <summary>
        /// Get available to user enumeration values of enum type.
        /// </summary>
        /// <param name="userID">User's ID.</param>
        /// <param name="enumerationTypeID">Enumeration Type's ID.</param>
        /// <returns>Collection of enumeration values.</returns>
        /// <exception cref="ANotFoundException">Thrown when user or connection info not found.</exception>
        /// <exception cref="DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<IEnumerable<EnumerationValueDto>> GetEnumerationVariants(ID<UserDto> userID, ID<EnumerationTypeDto> enumerationTypeID);
    }
}
