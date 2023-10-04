using System.Collections.Generic;

namespace Brio.Docs.Database.Models
{
    public class EnumerationValue
    {
        public int ID { get; set; }

        public string ExternalId { get; set; }

        public string Value { get; set; }

        public int EnumerationTypeID { get; set; }

        public EnumerationType EnumerationType { get; set; }

        public ICollection<ConnectionInfoEnumerationValue> ConnectionInfos { get; set; }
    }
}
