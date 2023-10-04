using System.Collections.Generic;

namespace Brio.Docs.Database.Models
{
    public class DynamicFieldInfo : IDynamicField
    {
        public int ID { get; set; }

        public string ExternalID { get; set; }

        public string Type { get; set; }

        public string Name { get; set; }

        public string Value { get; set; }

        public int? ObjectiveTypeID { get; set; }

        public ObjectiveType ObjectiveType { get; set; }

        public int? ParentFieldID { get; set; }

        public DynamicFieldInfo ParentField { get; set; }

        public int? ConnectionInfoID { get; set; }

        public ConnectionInfo ConnectionInfo { get; set; }

        public ICollection<DynamicFieldInfo> ChildrenDynamicFields { get; set; }
    }
}
