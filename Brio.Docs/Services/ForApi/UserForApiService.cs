using Brio.Docs.Client.Dtos;
using Brio.Docs.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brio.Docs.Client.Services.ForApi;
using DocumentFormat.OpenXml.Spreadsheet;
using AutoMapper;
using DocumentFormat.OpenXml.InkML;
using System.Net.Http;
using Brio.Docs.Database;
using Newtonsoft.Json;
using Brio.Docs.Client.Dtos.ForApi.Users;
using Brio.Docs.Client.Services.ForApi.Helpers;

namespace Brio.Docs.Services.ForApi
{
    public class UserForApiService : IUserForApiService
    {
        private readonly IMapper mapper;
        private readonly IHttpRequestForApiHandlerService httService;

        public UserForApiService(DMContext context, IMapper mapper, IHttpRequestForApiHandlerService httService)
        {

            this.httService = httService;
            this.mapper = mapper;
        }

        /// <summary>
        /// Get all registered users.
        /// </summary>
        /// <returns>A IEnumerable of <see cref="UserDto"/> representing the list of users.</returns>
        /// <exception cref="DocumentManagementException">Thrown when something went wrong.</exception>
        public async Task<IEnumerable<UserDto>> GetAllUsers()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Add new user.
        /// </summary>
        /// <param name="data">New user data.</param>
        /// <returns>Identifier of new user.</returns>
        /// <exception cref="ArgumentValidationException">Thrown when user data is invalid.</exception>
        /// <exception cref="DocumentManagementException">Thrown when something else went wrong.</exception>
        public async Task<ID<UserDto>> Add(UserToCreateDto data)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Delete user.
        /// </summary>
        /// <param name="userID">User ID to be deleted.</param>
        /// <returns>True, if user was deleted.</returns>
        /// <exception cref="ANotFoundException">Thrown when user not found.</exception>
        /// <exception cref="DocumentManagementException">Thrown when something else went wrong.</exception>
        public async Task<bool> Delete(ID<UserDto> userID)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Update user data.
        /// </summary>
        /// <param name="user">User data.</param>
        /// <returns>True, if user was updated.</returns>
        /// <exception cref="ANotFoundException">Thrown when user not found.</exception>
        /// <exception cref="ArgumentValidationException">Thrown when user with the same login already exists.</exception>
        /// <exception cref="DocumentManagementException">Thrown when something else went wrong.</exception>
        public async Task<bool> Update(UserDto user)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Check if password valid for specified user.
        /// </summary>
        /// <param name="userID">User's ID.</param>
        /// <param name="password">Password to verify.</param>
        /// <returns>True if password was verified successfully.</returns>
        /// <exception cref="ANotFoundException">Thrown when user not found.</exception>
        /// <exception cref="ArgumentValidationException">Thrown when password is invalid.</exception>
        /// <exception cref="DocumentManagementException">Thrown when something else went wrong.</exception>
        public async Task<bool> VerifyPassword(ID<UserDto> userID, string password)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Set new password for specified user.
        /// </summary>
        /// <returns>True if password was successfully updated.</returns>
        /// <param name="userID">User's ID.</param>
        /// <param name="newPass">New password.</param>
        /// <exception cref="ANotFoundException">Thrown when user not found.</exception>
        /// <exception cref="DocumentManagementException">Thrown when something else went wrong.</exception>
        public async Task<bool> UpdatePassword(ID<UserDto> userID, string newPass)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Query user data by user ID.
        /// </summary>
        /// <param name="userID">User's ID.</param>
        /// <returns>Found User.</returns>
        /// <exception cref="ANotFoundException">Thrown when user not found.</exception>
        /// <exception cref="DocumentManagementException">Thrown when something else went wrong.</exception>
        public async Task<UserDto> Find(int userID)
        {
            var response = await httService.SendGetRequest($"api/users/{userID}");

            if (response.IsSuccessStatusCode)
            {
                var cont = await response.Content.ReadAsStringAsync();

                var userFromApi = JsonConvert.DeserializeObject<UserForApiDto>(cont);
                var userForReturn = new UserDto((ID<UserDto>)userFromApi.Id, userFromApi.Login, userFromApi.Name);
                return userForReturn;
            }
            else
            {
                throw new Exception("Exception when finding user.");
            }
        }

        /// <summary>
        /// Query user data by user login.
        /// </summary>
        /// <param name="login">User's login.</param>
        /// <returns>Null if user with specified login was not found</returns>
        /// <exception cref="ANotFoundException">Thrown when user not found.</exception>
        /// <exception cref="DocumentManagementException">Thrown when something else went wrong.</exception>
        public async Task<UserDto> Find(string login)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Check if user with specified ID exists.
        /// </summary>
        /// <param name="userID">User's ID.</param>
        /// <returns>True if user exists, returns false otherwise.</returns>
        /// <exception cref="DocumentManagementException">Thrown when something went wrong.</exception>
        public async Task<bool> Exists(int userID)
        {
            var response = await httService.SendGetRequest($"api/users/{userID}");
            return response.IsSuccessStatusCode;
        }

        /// <summary>   
        /// Check if user with specified login exists.
        /// </summary>
        /// <param name="login">User's login.</param>
        /// <returns>True if user exists, returns false otherwise.</returns>
        /// <exception cref="DocumentManagementException">Thrown when something went wrong.</exception>
        public async Task<bool> Exists(string login)
        {
            // TODO: manage this case in API

            return true;
        }

        /// <summary>
        /// TEMPORARY: Sets current user for later use.
        /// </summary>
        /// <param name="userID">User's id</param>
        /// <returns>True, if user exists.</returns>
        public async Task<bool> SetCurrent(int userID)
        {
            if (!await Exists(userID))
                return false;

            CurrentUser.ID = userID;
            return true;
        }
    }
}
