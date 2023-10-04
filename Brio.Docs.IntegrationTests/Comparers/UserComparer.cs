using Brio.Docs.Client.Dtos;

namespace Brio.Docs.Tests
{
    internal class UserComparer : AbstractModelComparer<UserDto>
    {
        public UserComparer(bool ignoreIDs)
            : base(ignoreIDs)
        {
        }

        public override bool NotNullEquals(UserDto x, UserDto y)
        {
            var dataEqual = x.Login == y.Login && x.Name == y.Name;
            if (IgnoreIDs)
                return dataEqual;

            return dataEqual && x.ID == y.ID;
        }
    }
}
