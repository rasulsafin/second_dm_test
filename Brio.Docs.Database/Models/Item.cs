using System;
using System.Collections.Generic;

namespace Brio.Docs.Database.Models
{
    public class Item : ISynchronizable<Item>
    {
        public int ID { get; set; }

        public string RelativePath { get; set; }

        public string Name { get; set; }

        public int ItemType { get; set; }

        public string ExternalID { get; set; }

        public int? ProjectID { get; set; }

        public Project Project { get; set; }

        public ICollection<ObjectiveItem> Objectives { get; set; }

        public DateTime UpdatedAt { get; set; }

        public bool IsSynchronized { get; set; }

        public int? SynchronizationMateID { get; set; }

        public Item SynchronizationMate { get; set; }
    }
}
