using System;
using System.Collections.Generic;

namespace Brio.Docs.Reports.Models
{
    public class ReportDetails
    {
        public string ReportNumber { get; set; }

        public DateTime CreationTime { get; set; }

        public Dictionary<string, string> Fields { get; set; } = new Dictionary<string, string>();

        public string this[string key]
        {
            get
            {
                if (Fields == null)
                    return string.Empty;

                return Fields.TryGetValue(key, out var value) ? value : string.Empty;
            }
        }
    }
}
