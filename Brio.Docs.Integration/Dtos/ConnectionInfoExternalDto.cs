using System.Collections.Generic;
using Brio.Docs.Common.Dtos;

namespace Brio.Docs.Integration.Dtos
{
    public class ConnectionInfoExternalDto : IConnectionInfoDto
    {
        public ConnectionTypeExternalDto ConnectionType { get; set; }

        public IDictionary<string, string> AuthFieldValues { get; set; }

        public ICollection<EnumerationTypeExternalDto> EnumerationTypes { get; set; }

        public string UserExternalID { get; set; }
    }
}
