using Brio.Docs.Client;
using Brio.Docs.Common.Dtos;

namespace Brio.Docs.HttpConnection.Models
{
    public class Item
    {
        public ID<Item> ID { get; set; }

        public string FileName { get; set; }

        public string RelativePath { get; set; }

        public string ExternalItemId { get; set; }

        public ItemType ItemType { get; set; }
    }
}
