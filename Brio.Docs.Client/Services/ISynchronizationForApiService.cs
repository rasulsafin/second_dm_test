using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Brio.Docs.Client.Dtos;

namespace Brio.Docs.Client.Services
{
    public interface ISynchronizationForApiService
    {
        // <summary>
        /// Synchronize user's data.
        /// </summary>
        /// <param name="userID">User's ID.</param>
        /// <returns>Id of the created long request.</returns>
        /// <exception cref="ANotFoundException">Thrown when user not found.</exception>
        /// <exception cref="DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<RequestID> Synchronize(ID<UserDto> userID);

        /// <summary>
        /// Gets the dates of synchronizations for the user.
        /// </summary>
        /// <param name="userID">User's ID.</param>
        /// <returns>The date of the last synchronization or null if user is not synchronized.</returns>
        /// <exception cref="ANotFoundException">Thrown when user not found.</exception>
        /// <exception cref="DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<IEnumerable<DateTime>> GetSynchronizationDates(ID<UserDto> userID);

        /// <summary>
        /// Removes the last synchronization date of the user for an attempt to sync entities that were updated earlier than the last sync date.
        /// The entities will not be returned to the previous state.
        /// </summary>
        /// <param name="userID">User's ID.</param>
        /// <returns>True, if the last synchronization date is removed.</returns>
        /// <exception cref="ANotFoundException">Thrown when user not found.</exception>
        /// <exception cref="DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<bool> RemoveLastSynchronizationDate(ID<UserDto> userID);

        /// <summary>
        /// Removes all synchronization dates of the user for an attempt to synchronize all data.
        /// The entities will not be returned to the previous state.
        /// </summary>
        /// <param name="userID">User's ID.</param>
        /// <returns>True, if synchronization dates are removed.</returns>
        /// <exception cref="ANotFoundException">Thrown when user not found.</exception>
        /// <exception cref="DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<bool> RemoveAllSynchronizationDates(ID<UserDto> userID);
    }
}
