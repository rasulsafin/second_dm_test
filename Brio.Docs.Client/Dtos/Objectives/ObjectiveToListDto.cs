using System;
using System.Collections.Generic;
using Brio.Docs.Common;

namespace Brio.Docs.Client.Dtos
{
    public struct ObjectiveToListDto
    {
        public ID<ObjectiveDto> ID { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public ObjectiveStatus Status { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime DueDate { get; set; }

        public DateTime UpdatedAt { get; set; }

        public ObjectiveTypeDto ObjectiveType { get; set; }

        public IEnumerable<BimElementDto> BimElements { get; set; }

        public LocationDto Location { get; set; }

        public ID<ObjectiveDto>? ParentObjectiveID { get; set; }
    }
}
