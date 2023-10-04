using System;
using System.Collections.Generic;

namespace Brio.Docs.Reports.Models
{
    public class ObjectiveDetails
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public UserDetails Author { get; set; }

        public DateTime CreationTime { get; set; }

        public DateTime DueTime { get; set; }

        public List<AttachedElementDetails> AttachedElements { get; set; } = new List<AttachedElementDetails>();

        public List<AttachedImageDetails> AttachedImages { get; set; } = new List<AttachedImageDetails>();

        public Dictionary<string, string> Fields { get; set; } = new Dictionary<string, string>();
    }
}
