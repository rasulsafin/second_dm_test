using Brio.Docs.Common;

namespace Brio.Docs.HttpConnection.Models
{
    public class Location
    {
        public int ID { get; set; }

        public Vector3d Position { get; set; }

        public Vector3d CameraPosition { get; set; }

        public string Guid { get; set; }

        public Item Item { get; set; }
    }
}
