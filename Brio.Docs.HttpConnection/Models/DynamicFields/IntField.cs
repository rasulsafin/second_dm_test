using System.Globalization;
using Brio.Docs.Client;
using Brio.Docs.Common.Dtos;

namespace Brio.Docs.HttpConnection.Models
{
    public class IntField : IDynamicField
    {
        private int value;

        public ID<IDynamicField> ID { get; set; }

        public string Key { get; set; }

        public DynamicFieldType Type { get => DynamicFieldType.INTEGER; }

        public string Name { get; set; }

        public object Value
        {
            get => value;
            set => this.value = System.Convert.ToInt32(value, CultureInfo.InvariantCulture);
        }
    }
}
