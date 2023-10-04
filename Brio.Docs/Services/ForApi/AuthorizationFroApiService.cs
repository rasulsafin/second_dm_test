using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Brio.Docs.Client;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Client.Dtos.ForApi;
using Brio.Docs.Client.Dtos.ForApi.Users;
using Brio.Docs.Client.Services.ForApi;
using Brio.Docs.Client.Services.ForApi.Helpers;
using Brio.Docs.Database;
using Newtonsoft.Json;

namespace Brio.Docs.Services.ForApi
{
    public class AuthorizationFroApiService : IAuthorizationForApiService
    {
        private readonly IMapper mapper;
        private readonly IHttpRequestForApiHandlerService httpService;

        public AuthorizationFroApiService(DMContext context, IMapper mapper, IHttpRequestForApiHandlerService httpService)
        {
            this.mapper = mapper;
            this.httpService = httpService;
        }

        public async Task<IEnumerable<string>> GetAllRoles()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> AddRole(ID<UserDto> userID, string role)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> RemoveRole(ID<UserDto> userID, string role)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<string>> GetUserRoles(ID<UserDto> userID)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> IsInRole(ID<UserDto> userID, string role)
        {
            throw new NotImplementedException();
        }

        public async Task<ValidatedUserDto> Login(string username, string password)
        {
            try
            {
                var authRequest = new AuthenticateRequestForApiDto()
                {
                    Login = username,
                    Password = password,
                };

                // TODO think of making bearer token somehow if it's needed
                var response = await httpService.SendPostRequest("api/users/authenticate", authRequest);
                var responseBody = await response.Content.ReadAsStringAsync();

                var userFromApi = JsonConvert.DeserializeObject<UserForApiDto>(responseBody);

                var userToReturn = new UserDto((ID<UserDto>)userFromApi.Id, userFromApi.Login, userFromApi.Name);

                var res = new ValidatedUserDto()
                {
                    IsValidationSuccessful = true,
                    User = userToReturn,
                };

                return res;
            }

            // TODO: Make exception handling and etc.
            catch(Exception ex) 
            {
                throw;
            }
        }
    }
}
