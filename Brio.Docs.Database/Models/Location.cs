namespace Brio.Docs.Database.Models
{
    public class Location
    {
        public int ID { get; set; }

        public double PositionX { get; set; }

        public double PositionY { get; set; }

        public double PositionZ { get; set; }

        public double CameraPositionX { get; set; }

        public double CameraPositionY { get; set; }

        public double CameraPositionZ { get; set; }

        public string Guid { get; set; }

        public int ItemID { get; set; }

        public Item Item { get; set; }

        public int ObjectiveID { get; set; }

        public Objective Objective { get; set; }
    }
}
