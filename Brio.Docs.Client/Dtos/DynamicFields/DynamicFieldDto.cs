using Brio.Docs.Common.Dtos;

namespace Brio.Docs.Client.Dtos
{
    public class DynamicFieldDto
    {
        public ID<DynamicFieldDto> ID { get; set; }

        public DynamicFieldType Type { get; set; }

        public string Key { get; set; }

        public string Name { get; set; }

        public object Value { get; set; }
    }
}
