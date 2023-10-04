using System.Collections.Generic;
using Brio.Docs.Common.Dtos;

namespace Brio.Docs.Integration.Dtos
{
    public class DynamicFieldInfoExternalDto
    {
        public string ExternalID { get; set; }

        public DynamicFieldType Type { get; set; }

        public string Name { get; set; }

        public ICollection<DynamicFieldInfoExternalDto> ChildrenDynamicFields { get; set; }
    }
}
