using System.Collections.Generic;

namespace Brio.Docs.Client.Dtos
{
    public class EnumerationTypeDto
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public ICollection<EnumerationValueDto> EnumerationValues { get; set; }
    }
}
