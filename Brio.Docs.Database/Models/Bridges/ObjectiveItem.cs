namespace Brio.Docs.Database.Models
{
    public class ObjectiveItem
    {
        public int ObjectiveID { get; set; }

        public Objective Objective { get; set; }

        public int ItemID { get; set; }

        public Item Item { get; set; }
    }
}
