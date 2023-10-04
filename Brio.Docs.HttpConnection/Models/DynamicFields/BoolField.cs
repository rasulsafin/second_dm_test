using Brio.Docs.Client;
using Brio.Docs.Common.Dtos;

namespace Brio.Docs.HttpConnection.Models
{
    public class BoolField : IDynamicField
    {
        private bool value = false;

        public ID<IDynamicField> ID { get; set; }

        public string Key { get; set; }

        public DynamicFieldType Type { get => DynamicFieldType.BOOL; }

        public string Name { get; set; }

        public object Value
        {
            get => value;
            set
            {
                if (!(value is bool v))
                    throw new System.ArgumentException();
                this.value = v;
            }
        }
    }
}
