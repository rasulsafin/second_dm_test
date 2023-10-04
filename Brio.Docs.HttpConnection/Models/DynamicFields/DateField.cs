using System;
using Brio.Docs.Client;
using Brio.Docs.Common.Dtos;

namespace Brio.Docs.HttpConnection.Models
{
    public class DateField : IDynamicField
    {
        private DateTime value;

        public ID<IDynamicField> ID { get; set; }

        public string Key { get; set; }

        public DynamicFieldType Type { get => DynamicFieldType.DATE; }

        public string Name { get; set; }

        public object Value
        {
            get => value;
            set
            {
                if (!(value is DateTime v))
                    throw new ArgumentException();
                this.value = v;
            }
        }
    }
}
