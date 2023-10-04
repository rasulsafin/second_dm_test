using Brio.Docs.Client;

namespace Brio.Docs.HttpConnection.Models
{
    public class User
    {
        public ID<User> ID { get; set; }

        public string Login { get; set; }

        public string Name { get; set; }

        public string ConnectionName { get; set; }
    }
}
