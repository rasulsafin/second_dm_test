using System;
using System.Collections.Generic;
using System.Text;

namespace Brio.Docs.Client.Dtos.ForApi.Items
{
    public class ItemForApiDto : BaseForApiDto
    {
        public string Name { get; set; }

        public string RelativePath { get; set; }

        public long? ProjectId { get; set; }
    }
}