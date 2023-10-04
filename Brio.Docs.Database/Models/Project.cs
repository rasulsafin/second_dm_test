using System;
using System.Collections.Generic;

namespace Brio.Docs.Database.Models
{
    public class Project : ISynchronizable<Project>
    {
        public int ID { get; set; }

        public string Title { get; set; }

        public ICollection<Objective> Objectives { get; set; }

        public ICollection<UserProject> Users { get; set; }

        public ICollection<Item> Items { get; set; }

        public string ExternalID { get; set; }

        public DateTime UpdatedAt { get; set; }

        public bool IsSynchronized { get; set; }

        public int? SynchronizationMateID { get; set; }

        public Project SynchronizationMate { get; set; }
    }
}
