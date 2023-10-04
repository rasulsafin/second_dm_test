using System.Collections.Generic;
using Brio.Docs.Client;

namespace Brio.Docs.HttpConnection.Models
{
    public class ObjectiveType
    {
        public ID<ObjectiveType> ID { get; set; }

        public string Name { get; set; }

        public ICollection<IDynamicField> DefaultDynamicFields { get; set; }
    }
}
