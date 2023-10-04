using System.ComponentModel.DataAnnotations;

namespace Brio.Docs.Client.Dtos
{
    public class UserDto
    {
        public static readonly UserDto Anonymous = new UserDto(ID<UserDto>.InvalidID, string.Empty, string.Empty);

        public UserDto(ID<UserDto> id, string login, string name)
        {
            ID = id;
            Login = login?.Trim();
            Name = name?.Trim();
        }

        [Required(ErrorMessage = "ValidationError_IdIsRequired")]
        public ID<UserDto> ID { get; }

        [Required(ErrorMessage = "ValidationError_LoginIsRequired")]
        public string Login { get; }

        public string Name { get; }

        // TODO: Make better implementation of connectionName.
        public string ConnectionName { get; set; } = "MinIoClient";

        public RoleDto Role { get; set; }
    }
}
