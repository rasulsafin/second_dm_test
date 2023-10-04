namespace Brio.Docs.Client.Dtos
{
    public class ValidatedUserDto
    {
        public static ValidatedUserDto Invalid = new ValidatedUserDto { User = UserDto.Anonymous };

        public bool IsValidationSuccessful { get; set; }

        public UserDto User { get; set; }
    }
}
