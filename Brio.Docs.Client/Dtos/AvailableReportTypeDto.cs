using System.Collections.Generic;

namespace Brio.Docs.Client.Dtos
{
    public struct AvailableReportTypeDto
    {
        public string ID { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public ICollection<AvailableReportFieldDto> Fields { get; set; }
    }
}
