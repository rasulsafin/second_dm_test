using System;
using System.Collections.Generic;

namespace Brio.Docs.Client.Filters
{
    public class ObjectiveFilterParameters : PageParameters
    {
        public List<int> TypeIds { get; set; }

        public string BimElementGuid { get; set; }

        public string TitlePart { get; set; }

        public int? ExceptChildrenOf { get; set; }

        public List<int> Statuses { get; set; }

        public DateTime? CreatedBefore { get; set; }

        public DateTime? CreatedAfter { get; set; }

        public DateTime? UpdatedBefore { get; set; }

        public DateTime? UpdatedAfter { get; set; }

        public DateTime? FinishedBefore { get; set; }

        public DateTime? FinishedAfter { get; set; }

        public List<DynamicFieldFilterValue> DynamicFieldValues { get; set; }

        public string BimElementParent { get; set; }

        public class DynamicFieldFilterValue
        {
            public string ExternalId { get; set; }

            public string Value { get; set; }
        }
    }
}
