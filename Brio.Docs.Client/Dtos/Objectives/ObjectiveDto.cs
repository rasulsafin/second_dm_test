using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Brio.Docs.Common;

namespace Brio.Docs.Client.Dtos
{
    public class ObjectiveDto
    {
        [Required(ErrorMessage = "ValidationError_IdIsRequired")]
        public ID<ObjectiveDto> ID { get; set; }

        public ID<ProjectDto> ProjectID { get; set; }

        public ID<ObjectiveDto>? ParentObjectiveID { get; set; }

        public ID<UserDto>? AuthorID { get; set; }

        public ID<ObjectiveTypeDto> ObjectiveTypeID { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime DueDate { get; set; }

        public DateTime UpdatedAt { get; set; }

        [Required(ErrorMessage = "ValidationError_ObjectiveNameIsRequired")]
        public string Title { get; set; }

        public string Description { get; set; }

        public ObjectiveStatus Status { get; set; }

        public LocationDto Location { get; set; }

        public ICollection<ItemDto> Items { get; set; }

        public ICollection<DynamicFieldDto> DynamicFields { get; set; }

        public ICollection<BimElementDto> BimElements { get; set; }
    }
}
