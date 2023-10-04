using System.Collections.Generic;

namespace Brio.Docs.Database.Models
{
    public class ConnectionInfo
    {
        public int ID { get; set; }

        public int ConnectionTypeID { get; set; }

        public ConnectionType ConnectionType { get; set; }

        public User User { get; set; }

        public ICollection<AuthFieldValue> AuthFieldValues { get; set; }

        public ICollection<ConnectionInfoEnumerationType> EnumerationTypes { get; set; }

        public ICollection<ConnectionInfoEnumerationValue> EnumerationValues { get; set; }
    }
}
