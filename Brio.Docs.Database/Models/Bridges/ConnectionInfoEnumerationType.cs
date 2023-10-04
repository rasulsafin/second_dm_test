namespace Brio.Docs.Database.Models
{
    public class ConnectionInfoEnumerationType
    {
        public int ConnectionInfoID { get; set; }

        public ConnectionInfo ConnectionInfo { get; set; }

        public int EnumerationTypeID { get; set; }

        public EnumerationType EnumerationType { get; set; }
    }
}
