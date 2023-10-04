using System.ComponentModel.DataAnnotations;

namespace Brio.Docs.Client.Dtos
{
    public struct UserToCreateDto
    {
        public UserToCreateDto(string login, string password, string name)
        {
            Login = login?.Trim();
            Password = password?.Trim();
            Name = name?.Trim();
        }

        [Required(ErrorMessage = "ValidationError_LoginIsRequired")]
        public string Login { get; }

        [Required(ErrorMessage = "ValidationError_PasswordIsRequired")]
        public string Password { get; }

        public string Name { get; }
    }
}
