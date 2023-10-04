using System.Collections.Generic;
using Brio.Docs.Client.Dtos.ForApi.Project;

namespace Brio.Docs.Client.Dtos.ForApi.Users
{
    public class UserToReadForApi : UserForApiDto
    {
        public ICollection<int> ProjectsIds { get; set; }

        public ICollection<ProjectForApiDto> Projects { get; set; }
    }
}
