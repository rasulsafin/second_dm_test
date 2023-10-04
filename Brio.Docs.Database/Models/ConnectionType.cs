using System.Collections.Generic;

namespace Brio.Docs.Database.Models
{
    public class ConnectionType
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public IEnumerable<AppProperty> AppProperties { get; set; }

        public IEnumerable<AuthFieldName> AuthFieldNames { get; set; }

        public ICollection<ConnectionInfo> ConnectionInfos { get; set; }

        public ICollection<ObjectiveType> ObjectiveTypes { get; set; }

        public ICollection<EnumerationType> EnumerationTypes { get; set; }
    }
}
