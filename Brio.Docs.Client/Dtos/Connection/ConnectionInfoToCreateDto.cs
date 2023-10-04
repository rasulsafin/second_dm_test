using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Brio.Docs.Common.Dtos;

namespace Brio.Docs.Client.Dtos
{
    public class ConnectionInfoToCreateDto : IConnectionInfoDto
    {
        [Required(ErrorMessage = "ValidationError_IdIsRequired")]
        public ID<ConnectionTypeDto> ConnectionTypeID { get; set;  }

        [Required(ErrorMessage = "ValidationError_IdIsRequired")]
        public ID<UserDto> UserID { get; set;  }

        public IDictionary<string, string> AuthFieldValues { get; set;  }
    }
}
