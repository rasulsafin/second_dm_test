using System.Collections.Generic;
using System.Threading.Tasks;
using Brio.Docs.Client;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Client.Services;

namespace Brio.Docs.HttpConnection.Services
{
    internal class UserService : ServiceBase, IUserService
    {
        private static readonly string PATH = "Users";

        public UserService(Connection connection)
            : base(connection)
        {
        }

        public async Task<ID<UserDto>> Add(UserToCreateDto data)
            => await Connection.PostObjectJsonAsync<UserToCreateDto, ID<UserDto>>($"{PATH}", data);

        public async Task<bool> Delete(ID<UserDto> userID)
            => await Connection.DeleteDataAsync<bool>($"{PATH}/{{0}}", userID);

        public async Task<bool> Exists(ID<UserDto> userID)
            => await Connection.GetDataAsync<bool>($"{PATH}/exists/{{0}}", userID);

        public async Task<bool> Exists(string login)
            => await Connection.GetDataQueryAsync<bool>($"{PATH}/exists", $"login ={{0}}", new object[] { login });

        public async Task<UserDto> Find(ID<UserDto> userID)
            => await Connection.GetDataAsync<UserDto>($"{PATH}/{{0}}", userID);

        public async Task<UserDto> Find(string login)
            => await Connection.GetDataQueryAsync<UserDto>($"{PATH}/find", $"login={{0}}", new object[] { login });

        public async Task<IEnumerable<UserDto>> GetAllUsers()
            => await Connection.GetDataAsync<IEnumerable<UserDto>>($"{PATH}");

        public async Task<bool> Update(UserDto user)
            => await Connection.PutObjectJsonAsync<UserDto, bool>($"{PATH}", user);

        public async Task<bool> UpdatePassword(ID<UserDto> userID, string newPass)
            => await Connection.PutObjectJsonAsync<string, bool>($"{PATH}/{{0}}/password", newPass, userID);

        public async Task<bool> VerifyPassword(ID<UserDto> userID, string password)
            => await Connection.PostObjectJsonAsync<string, bool>($"{PATH}/{{0}}/password", password, userID);

        public async Task<bool> SetCurrent(ID<UserDto> userID)
            => await Connection.PostObjectJsonAsync<ID<UserDto>, bool>($"{PATH}/current", userID);
    }
}
