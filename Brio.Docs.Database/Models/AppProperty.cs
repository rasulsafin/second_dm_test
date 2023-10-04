namespace Brio.Docs.Database.Models
{
    public class AppProperty
    {
        public int ID { get; set; }

        public string Key { get; set; }

        public string Value { get; set; }

        public int ConnectionTypeID { get; set; }

        public ConnectionType ConnectionType { get; set; }
    }
}
