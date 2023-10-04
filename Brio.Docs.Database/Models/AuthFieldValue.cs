namespace Brio.Docs.Database.Models
{
    public class AuthFieldValue
    {
        public int ID { get; set; }

        public string Key { get; set; }

        public string Value { get; set; }

        public int ConnectionInfoID { get; set; }

        public ConnectionInfo ConnectionInfo { get; set; }
    }
}
