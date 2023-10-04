using System.Collections.Generic;
using Brio.Docs.Client;
using Brio.Docs.Common.Dtos;

namespace Brio.Docs.HttpConnection.Models
{
    public class ObjectDynamicField : IDynamicField
    {
        private ICollection<IDynamicField> value;

        public ID<IDynamicField> ID { get; set; }

        public string Key { get; set; }

        public DynamicFieldType Type { get => DynamicFieldType.OBJECT; }

        public string Name { get; set; }

        public object Value
        {
            get => value;
            set
            {
                if (!(value is ICollection<IDynamicField> v))
                    throw new System.ArgumentException();
                this.value = v;
            }
        }
    }
}
