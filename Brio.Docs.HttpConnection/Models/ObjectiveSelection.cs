using System.Collections.Generic;
using Brio.Docs.Client;

namespace Brio.Docs.HttpConnection.Models
{
    public class ObjectiveSelection
    {
        public ID<Objective> ID { get; set; }

        public ICollection<BimElement> BimElements { get; set; }
    }
}
