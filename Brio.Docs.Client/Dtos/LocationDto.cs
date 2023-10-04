namespace Brio.Docs.Client.Dtos
{
    public class LocationDto
    {
        public int ID { get; set; }

        public (double x, double y, double z) Position { get; set; }

        public (double x, double y, double z) CameraPosition { get; set; }

        public string Guid { get; set; }

        public ItemDto Item { get; set; }
    }
}
