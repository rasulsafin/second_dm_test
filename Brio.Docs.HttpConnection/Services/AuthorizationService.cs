using System.Collections.Generic;
using System.Threading.Tasks;
using Brio.Docs.Client;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Client.Services;

namespace Brio.Docs.HttpConnection.Services
{
    internal class AuthorizationService : ServiceBase, IAuthorizationService
    {
        private static readonly string PATH = "Authorizations";

        public AuthorizationService(Connection connection)
            : base(connection)
        {
        }

        public Task<bool> AddRole(ID<UserDto> userID, string role)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<string>> GetAllRoles()
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<string>> GetUserRoles(ID<UserDto> userID)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> IsInRole(ID<UserDto> userID, string role)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> RemoveRole(ID<UserDto> userID, string role)
        {
            throw new System.NotImplementedException();
        }

        public async Task<ValidatedUserDto> Login(string username, string password)
        {
            var (authorizationToken, user) = await Connection.PostObjectJsonQueryAsync<string, (string, ValidatedUserDto)>($"{PATH}/login", $"username={{0}}", new object[] { username }, password);

            if (user == null)
                return ValidatedUserDto.Invalid;

            return user;
        }
    }
}
