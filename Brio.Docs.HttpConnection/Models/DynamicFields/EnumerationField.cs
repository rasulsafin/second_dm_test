using Brio.Docs.Client;
using Brio.Docs.Common.Dtos;

namespace Brio.Docs.HttpConnection.Models
{
    public class EnumerationField : IDynamicField
    {
        private EnumerationValue value;

        public ID<IDynamicField> ID { get; set; }

        public string Key { get; set; }

        public DynamicFieldType Type { get => DynamicFieldType.ENUM; }

        public string Name { get; set; }

        public EnumerationType EnumerationType { get; set; }

        public object Value
        {
            get => value;
            set
            {
                if (!(value is EnumerationValue v))
                    throw new System.ArgumentException();
                this.value = v;
            }
        }
    }
}
