using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Brio.Docs.Common;

namespace Brio.Docs.Client.Dtos
{
    public struct ObjectiveToCreateDto
    {
        public ID<UserDto>? AuthorID { get; set; }

        [Required(ErrorMessage = "ValidationError_IdIsRequired")]
        public ID<ProjectDto> ProjectID { get; set; }

        public ID<ObjectiveDto>? ParentObjectiveID { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime DueDate { get; set; }

        [Required(ErrorMessage = "ValidationError_ObjectiveNameIsRequired")]
        public string Title { get; set; }

        public string Description { get; set; }

        public ObjectiveStatus Status { get; set; }

        public LocationDto Location { get; set; }

        [Required(ErrorMessage = "ValidationError_IdIsRequired")]
        public ID<ObjectiveTypeDto> ObjectiveTypeID { get; set; }

        public IEnumerable<ItemDto> Items { get; set; }

        public ICollection<DynamicFieldDto> DynamicFields { get; set; }

        public IEnumerable<BimElementDto> BimElements { get; set; }
    }
}
