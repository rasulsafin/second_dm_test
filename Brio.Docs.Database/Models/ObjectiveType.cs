using System.Collections.Generic;

namespace Brio.Docs.Database.Models
{
    public class ObjectiveType
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public string ExternalId { get; set; }

        public int? ConnectionTypeID { get; set; }

        public ConnectionType ConnectionType { get; set; }

        public ICollection<Objective> Objectives { get; set; }

        public ICollection<DynamicFieldInfo> DefaultDynamicFields { get; set; }
    }
}
