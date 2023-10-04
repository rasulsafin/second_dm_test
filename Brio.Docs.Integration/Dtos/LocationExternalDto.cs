namespace Brio.Docs.Integration.Dtos
{
    public class LocationExternalDto
    {
        public (double x, double y, double z) Location { get; set; }

        public (double x, double y, double z) CameraPosition { get; set; }

        public string Guid { get; set; }

        public ItemExternalDto Item { get; set; }
    }
}
