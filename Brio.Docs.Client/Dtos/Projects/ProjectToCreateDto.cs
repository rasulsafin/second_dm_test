using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Brio.Docs.Client.Dtos
{
    public struct ProjectToCreateDto
    {
        public ID<UserDto> AuthorID { get; set; }

        [Required(ErrorMessage = "ValidationError_ProjectNameIsRequired")]
        public string Title { get; set; }

        public IEnumerable<ItemDto> Items { get; set; }
    }
}
