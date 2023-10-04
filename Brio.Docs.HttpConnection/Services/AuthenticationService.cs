using System.Threading.Tasks;
using Brio.Docs.Client.Dtos;

namespace Brio.Docs.HttpConnection.Services
{
    public class AuthenticationService
    {
        private static readonly string PATH = "Authorizations";
        private readonly Connection connection;

        public AuthenticationService(Connection connection)
        {
            this.connection = connection;
        }

        public async Task<AuthenticationResult> Login(string username, string password)
        {
            var (token, user) = await connection.PostObjectJsonQueryAsync<string, (string, ValidatedUserDto)>($"{PATH}/login", $"username={{0}}", new object[] { username }, password);

            if (user != null && user.IsValidationSuccessful && user.User.ID.IsValid)
            {
                return new AuthenticationResult(true, user.User, token);
            }

            return new AuthenticationResult(false, UserDto.Anonymous, null);
        }

        public class AuthenticationResult
        {
            internal AuthenticationResult(bool isValidationSuccessful, UserDto user, string authenticationToken)
            {
                User = user;
                IsValidationSuccessful = isValidationSuccessful;
                AuthenticationToken = authenticationToken;
            }

            public bool IsValidationSuccessful { get; }

            public UserDto User { get; }

            public string AuthenticationToken { get; }
        }
    }
}
