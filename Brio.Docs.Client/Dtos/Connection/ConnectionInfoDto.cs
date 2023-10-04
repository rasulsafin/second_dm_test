using System.Collections.Generic;
using Brio.Docs.Common.Dtos;

namespace Brio.Docs.Client.Dtos
{
    public class ConnectionInfoDto : IConnectionInfoDto
    {
        public ID<ConnectionInfoDto> ID { get; set; }

        public ConnectionTypeDto ConnectionType { get; set; }

        public IDictionary<string, string> AuthFieldValues { get; set; }

        public ICollection<EnumerationTypeDto> EnumerationTypes { get; set; }
    }
}
