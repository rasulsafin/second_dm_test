using System;
using System.Collections.Generic;

namespace Brio.Docs.Database.Models
{
    public class Objective : ISynchronizable<Objective>
    {
        public int ID { get; set; }

        public int ProjectID { get; set; }

        public Project Project { get; set; }

        public int? ParentObjectiveID { get; set; }

        public Objective ParentObjective { get; set; }

        public int? AuthorID { get; set; }

        public User Author { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime DueDate { get; set; }

        public string Title { get; set; }

        public string TitleToLower { get; set; }

        public string Description { get; set; }

        public int Status { get; set; }

        public int ObjectiveTypeID { get; set; }

        public Location Location { get; set; }

        public ObjectiveType ObjectiveType { get; set; }

        public ICollection<Objective> ChildrenObjectives { get; set; }

        public ICollection<DynamicField> DynamicFields { get; set; }

        public ICollection<ObjectiveItem> Items { get; set; }

        public ICollection<BimElementObjective> BimElements { get; set; }

        public string ExternalID { get; set; }

        public DateTime UpdatedAt { get; set; }

        public bool IsSynchronized { get; set; }

        public int? SynchronizationMateID { get; set; }

        public Objective SynchronizationMate { get; set; }
    }
}
