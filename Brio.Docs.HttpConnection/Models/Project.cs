using System.Collections.Generic;
using Brio.Docs.Client;

namespace Brio.Docs.HttpConnection.Models
{
    public class Project
    {
        public ID<Project> ID { get; set; }

        public string Title { get; set; }

        public IEnumerable<Item> Items { get; set; }
    }
}
