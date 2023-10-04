using System;
using System.Collections.Generic;
using System.Text;

namespace Brio.Docs.Client.Dtos.ForApi.Users
{
    public class UserForApiDto : BaseForApiDto
    {
        public string Name { get; set; }

        public string LastName { get; set; }

        public string FathersName { get; set; }

        public string Login { get; set; }

        public string Email { get; set; }

        public string HashedPassword { get; set; }

        public string Position { get; set; }

        public long RoleId { get; set; }

        public long OrganizationId { get; set; }
    }
}
