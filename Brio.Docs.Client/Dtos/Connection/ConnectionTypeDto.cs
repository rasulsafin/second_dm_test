using System.Collections.Generic;
using Brio.Docs.Common.Dtos;

namespace Brio.Docs.Client.Dtos
{
    public class ConnectionTypeDto : IConnectionTypeDto
    {
        public ID<ConnectionTypeDto> ID { get; set; }

        public string Name { get; set; }

        public IDictionary<string, string> AppProperties { get; set; }

        public IEnumerable<string> AuthFieldNames { get; set; }

        public ICollection<ObjectiveTypeDto> ObjectiveTypes { get; set; }

        public ICollection<EnumerationTypeDto> EnumerationTypes { get; set; }
    }
}
