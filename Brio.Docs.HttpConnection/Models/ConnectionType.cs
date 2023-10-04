using System.Collections.Generic;
using Brio.Docs.Client;

namespace Brio.Docs.HttpConnection.Models
{
    public class ConnectionType
    {
        public ID<ConnectionType> ID { get; set; }

        public string Name { get; set; }

        public IEnumerable<string> AuthFieldNames { get; set; }
    }
}
