using Brio.Docs.Common.ForApi;

namespace Brio.Docs.Client.Dtos.ForApi.Fields
{
    public class FieldForApiDto
    {
        public FieldForApiDto()
        {
            Type = FieldEnum.Text;
            IsMandatory = false;
        }

        public string Name { get; set; }

        public bool IsMandatory { get; set; }

        public string Data { get; set; }

        public FieldEnum Type { get; set; }

        public long? RecordId { get; set; }

        public long? TemplateId { get; set; }
    }
}
