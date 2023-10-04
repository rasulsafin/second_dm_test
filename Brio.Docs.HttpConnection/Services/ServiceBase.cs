namespace Brio.Docs.HttpConnection.Services
{
    internal class ServiceBase
    {
        public ServiceBase(Connection connection)
        {
            Connection = connection;
        }

        protected Connection Connection { get; }
    }
}
