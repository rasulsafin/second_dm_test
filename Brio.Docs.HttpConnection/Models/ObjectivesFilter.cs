using System;
using System.Collections.Generic;
using System.Linq;
using Brio.Docs.Client;
using Brio.Docs.Client.Filters;

namespace Brio.Docs.HttpConnection.Models
{
    public class ObjectivesFilter : IReadonlyObjectivesFilter
    {
        public string TitlePart { get; set; }

        public string BimElementGuid { get; set; }

        public int? ExceptChildrenOf { get; set; }

        public List<ID<ObjectiveType>> TypeIds { get; set; } = new List<ID<ObjectiveType>>();

        public List<int> Statuses { get; set; } = new List<int>();

        public DateTime? CreatedBefore { get; set; }

        public DateTime? CreatedAfter { get; set; }

        public DateTime? UpdatedBefore { get; set; }

        public DateTime? UpdatedAfter { get; set; }

        public DateTime? FinishedBefore { get; set; }

        public DateTime? FinishedAfter { get; set; }

        public string BimElementParent { get; set; }

        public List<ObjectiveFilterParameters.DynamicFieldFilterValue> DynamicFieldValues { get; set; } =
            new List<ObjectiveFilterParameters.DynamicFieldFilterValue>();

        IReadOnlyList<ID<ObjectiveType>> IReadonlyObjectivesFilter.TypeIds => TypeIds;

        IReadOnlyList<int> IReadonlyObjectivesFilter.Statuses => Statuses;

        IReadOnlyList<ObjectiveFilterParameters.DynamicFieldFilterValue> IReadonlyObjectivesFilter.DynamicFieldValues =>
            DynamicFieldValues;

        public void CopyValuesFrom(IReadonlyObjectivesFilter other)
        {
            TitlePart = other.TitlePart;
            BimElementGuid = other.BimElementGuid;
            ExceptChildrenOf = other.ExceptChildrenOf;
            TypeIds = other.TypeIds?.ToList();
            Statuses = other.Statuses?.ToList();
            CreatedAfter = other.CreatedAfter;
            CreatedBefore = other.CreatedBefore;
            UpdatedAfter = other.UpdatedAfter;
            UpdatedBefore = other.UpdatedBefore;
            FinishedAfter = other.FinishedAfter;
            FinishedBefore = other.FinishedBefore;
            DynamicFieldValues = other.DynamicFieldValues?.ToList();
            BimElementParent = other.BimElementParent;
        }

        public void RestoreDefaultValues() => CopyValuesFrom(new ObjectivesFilter());
    }
}
