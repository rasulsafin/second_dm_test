using System.Collections.Generic;

namespace Brio.Docs.HttpConnection.Models
{
    public class EnumerationType
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public ICollection<EnumerationValue> EnumerationValues { get; set; }
    }
}
