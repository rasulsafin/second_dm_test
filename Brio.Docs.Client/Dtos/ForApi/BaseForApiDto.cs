using System;
using System.Collections.Generic;
using System.Text;

namespace Brio.Docs.Client.Dtos.ForApi
{
    public class BaseForApiDto
    {
        public BaseForApiDto()
        {
            CreatedAt = DateTime.Now;
        }

        public long Id { get; set; }

        public long? CreatedById { get; set; }

        public long? UpdatedById { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
