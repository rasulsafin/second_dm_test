using Brio.Docs.Common;

namespace Brio.Docs.Client.Dtos
{
    public struct BimElementStatusDto
    {
        public string GlobalID { get; set; }

        public ObjectiveStatus Status { get; set; }
    }
}
