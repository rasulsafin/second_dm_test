using System.Collections.Generic;

namespace Brio.Docs.Database.Models
{
    public class EnumerationType
    {
        public int ID { get; set; }

        public string ExternalId { get; set; }

        public string Name { get; set; }

        public ConnectionType ConnectionType { get; set; }

        public ICollection<ConnectionInfoEnumerationType> ConnectionInfos { get; set; }

        public ICollection<EnumerationValue> EnumerationValues { get; set; }
    }
}
