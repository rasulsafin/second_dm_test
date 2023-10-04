using System.Globalization;
using Brio.Docs.Client;
using Brio.Docs.Common.Dtos;

namespace Brio.Docs.HttpConnection.Models
{
    public class FloatField : IDynamicField
    {
        private float value;

        public ID<IDynamicField> ID { get; set; }

        public string Key { get; set; }

        public DynamicFieldType Type { get => DynamicFieldType.FLOAT; }

        public string Name { get; set; }

        public object Value
        {
            get => value;
            set => this.value = System.Convert.ToSingle(value, CultureInfo.InvariantCulture);
        }
    }
}
