using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Brio.Docs.Client.Dtos
{
    public class ReportDto
    {
        [Required(ErrorMessage = "ValidationError_ObjectivesIsRequired")]
        public IEnumerable<ID<ObjectiveDto>> Objectives { get; set; }

        public IEnumerable<string> ScreenshotTypes { get; set; }

        public IDictionary<string, string> Fields { get; set; }
    }
}
