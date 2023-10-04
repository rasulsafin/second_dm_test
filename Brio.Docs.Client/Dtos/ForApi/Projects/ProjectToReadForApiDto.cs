using Brio.Docs.Client.Dtos.ForApi.Items;
using Brio.Docs.Client.Dtos.ForApi.Project;
using Brio.Docs.Client.Dtos.ForApi.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace Brio.Docs.Client.Dtos.ForApi.Projects
{
    public class ProjectToReadForApiDto : ProjectForApiDto
    {
        public ICollection<int> ItemIds { get; set; }

        public ICollection<ItemForApiDto> Items { get; set; }

        public ICollection<int?> UserIds { get; set; }

        public ICollection<UserForApiDto> Users { get; set; }
    }
}
