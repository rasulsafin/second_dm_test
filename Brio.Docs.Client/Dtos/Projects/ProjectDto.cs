using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Brio.Docs.Client.Dtos
{
    public class ProjectDto
    {
        [Required(ErrorMessage = "ValidationError_IdIsRequired")]
        public ID<ProjectDto> ID { get; set; }

        [Required(ErrorMessage = "ValidationError_ProjectNameIsRequired")]
        public string Title { get; set; }

        public IEnumerable<ItemDto> Items { get; set; }
    }
}
