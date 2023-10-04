using Brio.Docs.Client;
using Brio.Docs.Common.Dtos;

namespace Brio.Docs.HttpConnection.Models
{
    public class StringField : IDynamicField
    {
        private string value;

        public ID<IDynamicField> ID { get; set; }

        public string Key { get; set; }

        public DynamicFieldType Type { get => DynamicFieldType.STRING; }

        public string Name { get; set; }

        public object Value
        {
            get => value;
            set
            {
                if (value is null)
                    value = string.Empty;

                if (!(value is string v))
                    throw new System.ArgumentException();
                this.value = v;
            }
        }
    }
}
