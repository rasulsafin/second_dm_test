using Brio.Docs.Client.Dtos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Brio.Docs.Client.Services.ForApi
{
    public interface IUserForApiService
    {
        /// <summary>
        /// Get all registered users.
        /// </summary>
        /// <returns>A IEnumerable of <see cref="UserDto"/> representing the list of users.</returns>
        /// <exception cref="DocumentManagementException">Thrown when something went wrong.</exception>
        Task<IEnumerable<UserDto>> GetAllUsers();

        /// <summary>
        /// Add new user.
        /// </summary>
        /// <param name="data">New user data.</param>
        /// <returns>Identifier of new user.</returns>
        /// <exception cref="ArgumentValidationException">Thrown when user data is invalid.</exception>
        /// <exception cref="DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<ID<UserDto>> Add(UserToCreateDto data);

        /// <summary>
        /// Delete user.
        /// </summary>
        /// <param name="userID">User ID to be deleted.</param>
        /// <returns>True, if user was deleted.</returns>
        /// <exception cref="ANotFoundException">Thrown when user not found.</exception>
        /// <exception cref="DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<bool> Delete(ID<UserDto> userID);

        /// <summary>
        /// Update user data.
        /// </summary>
        /// <param name="user">User data.</param>
        /// <returns>True, if user was updated.</returns>
        /// <exception cref="ANotFoundException">Thrown when user not found.</exception>
        /// <exception cref="ArgumentValidationException">Thrown when user with the same login already exists.</exception>
        /// <exception cref="DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<bool> Update(UserDto user);

        /// <summary>
        /// Check if password valid for specified user.
        /// </summary>
        /// <param name="userID">User's ID.</param>
        /// <param name="password">Password to verify.</param>
        /// <returns>True if password was verified successfully.</returns>
        /// <exception cref="ANotFoundException">Thrown when user not found.</exception>
        /// <exception cref="ArgumentValidationException">Thrown when password is invalid.</exception>
        /// <exception cref="DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<bool> VerifyPassword(ID<UserDto> userID, string password);

        /// <summary>
        /// Set new password for specified user.
        /// </summary>
        /// <returns>True if password was successfully updated.</returns>
        /// <param name="userID">User's ID.</param>
        /// <param name="newPass">New password.</param>
        /// <exception cref="ANotFoundException">Thrown when user not found.</exception>
        /// <exception cref="DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<bool> UpdatePassword(ID<UserDto> userID, string newPass);

        /// <summary>
        /// Query user data by user ID.
        /// </summary>
        /// <param name="userID">User's ID.</param>
        /// <returns>Found User.</returns>
        /// <exception cref="ANotFoundException">Thrown when user not found.</exception>
        /// <exception cref="DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<UserDto> Find(int userID);

        /// <summary>
        /// Query user data by user login.
        /// </summary>
        /// <param name="login">User's login.</param>
        /// <returns>Null if user with specified login was not found</returns>
        /// <exception cref="ANotFoundException">Thrown when user not found.</exception>
        /// <exception cref="DocumentManagementException">Thrown when something else went wrong.</exception>
        Task<UserDto> Find(string login);

        /// <summary>
        /// Check if user with specified ID exists.
        /// </summary>
        /// <param name="userID">User's ID.</param>
        /// <returns>True if user exists, returns false otherwise.</returns>
        /// <exception cref="DocumentManagementException">Thrown when something went wrong.</exception>
        Task<bool> Exists(int userID);

        /// <summary>
        /// Check if user with specified login exists.
        /// </summary>
        /// <param name="login">User's login.</param>
        /// <returns>True if user exists, returns false otherwise.</returns>
        /// <exception cref="DocumentManagementException">Thrown when something went wrong.</exception>
        Task<bool> Exists(string login);

        /// <summary>
        /// TEMPORARY: Sets current user for later use.
        /// </summary>
        /// <param name="userID">User's id</param>
        /// <returns>True, if user exists.</returns>
        Task<bool> SetCurrent(int userID);
    }
}
