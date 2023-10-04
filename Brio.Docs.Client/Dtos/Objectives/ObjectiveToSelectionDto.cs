using System.Collections.Generic;

namespace Brio.Docs.Client.Dtos
{
    public struct ObjectiveToSelectionDto
    {
        public ID<ObjectiveDto> ID { get; set; }

        public IEnumerable<BimElementDto> BimElements { get; set; }
    }
}
