using System.Collections.Generic;

namespace Brio.Docs.Integration.Dtos
{
    public class ObjectiveTypeExternalDto
    {
        public string Name { get; set; }

        public string ExternalId { get; set; }

        public ICollection<DynamicFieldExternalDto> DefaultDynamicFields { get; set; }
    }
}
