using System.ComponentModel.DataAnnotations;
using Brio.Docs.Common;

namespace Brio.Docs.Client.Dtos
{
    public struct ObjectiveToLocationDto
    {
        [Required(ErrorMessage = "ValidationError_IdIsRequired")]
        public ID<ObjectiveDto> ID { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public ObjectiveStatus Status { get; set; }

        public LocationDto Location { get; set; }
    }
}
