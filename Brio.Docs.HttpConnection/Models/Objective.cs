using System;
using System.Collections.Generic;
using Brio.Docs.Client;
using Brio.Docs.Common;

namespace Brio.Docs.HttpConnection.Models
{
    public class Objective
    {
        public ID<Objective> ID { get; set; }

        public ID<Project> ProjectID { get; set; }

        public ID<Objective>? ParentObjectiveID { get; set; }

        public ID<User>? AuthorID { get; set; }

        public ObjectiveType ObjectiveType { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime DueDate { get; set; }

        public DateTime UpdatedAt { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public ObjectiveStatus Status { get; set; }

        public Location Location { get; set; }

        public IEnumerable<Item> Items { get; set; }

        public ICollection<IDynamicField> DynamicFields { get; set; }

        public ICollection<BimElement> BimElements { get; set; }

        public ICollection<Objective> Subobjectives { get; set; }
    }
}
