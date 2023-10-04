using System.ComponentModel.DataAnnotations;

namespace Brio.Docs.Client.Filters
{
    public class PageParameters
    {
        [Range(1, int.MaxValue, ErrorMessage = "ValidationError_PageNumberTooSmall")]
        public int PageNumber { get; set; } = 1;

        [Range(1, int.MaxValue, ErrorMessage = "ValidationError_PageSizeNotInRange")]
        public int PageSize { get; set; } = 10;
    }
}
