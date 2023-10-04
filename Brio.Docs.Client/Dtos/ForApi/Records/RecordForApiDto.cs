using System;
using System.Collections.Generic;
using Brio.Docs.Client.Dtos.ForApi.Fields;
using Brio.Docs.Common.ForApi;

namespace Brio.Docs.Client.Dtos.ForApi.Records
{
    public class RecordForApiDto : BaseForApiDto
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string Executor { get; set; }

        public bool IsInArchive { get; set; }

        // Дата устранения
        public DateTime FixDate { get; set; }

        public StatusEnum Status { get; set; }

        public PriorityEnum Priority { get; set; }

        public long ProjectId { get; set; }

        public long? TemplateId { get; set; }

        public long? ParentId { get; set; }

        public ICollection<RecordForApiDto> ChildRecords { get; set; }

        public ICollection<int> FieldIds { get; set; }

        public ICollection<FieldForApiDto> Fields { get; set; }

        public ICollection<int> ListFieldIds { get; set; }

        //TODO
        /*public ICollection<ListFieldDto> ListFields { get; set; }*/
    }
}
