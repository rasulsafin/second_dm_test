using System.Collections.Generic;

namespace Brio.Docs.Client.Dtos
{
    public class ObjectiveTypeDto
    {
        public ID<ObjectiveTypeDto> ID { get; set; }

        public string Name { get; set; }

        public ICollection<DynamicFieldDto> DefaultDynamicFields { get; set; }
    }
}
