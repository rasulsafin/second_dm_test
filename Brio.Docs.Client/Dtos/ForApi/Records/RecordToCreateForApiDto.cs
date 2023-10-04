using System;
using System.Collections.Generic;
using System.Text;

namespace Brio.Docs.Client.Dtos.ForApi.Records
{
    public class RecordToCreateForApiDto : RecordForApiDto
    {
        public ICollection<int> ListChildIds { get; set; }
    }
}
