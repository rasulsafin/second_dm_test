using System;
using System.Collections.Generic;
using Brio.Docs.Client.Dtos.ForApi.Items;
using Brio.Docs.Client.Dtos.ForApi.Users;

namespace Brio.Docs.Client.Dtos.ForApi.Project
{
    public class ProjectToCreateForApiDto : ProjectForApiDto
    {
        public ProjectToCreateForApiDto()
        {
            UpdatedAt = DateTime.Now;
        }

        public ICollection<int> ItemIds { get; set; }

        public ICollection<ItemForApiDto> Items { get; set; }

        public ICollection<int> UserIds { get; set; }

        public ICollection<UserForApiDto> Users { get; set; }
    }
}
