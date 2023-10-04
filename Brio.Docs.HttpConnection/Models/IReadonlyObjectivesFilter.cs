using System;
using System.Collections.Generic;
using Brio.Docs.Client;
using Brio.Docs.Client.Filters;

namespace Brio.Docs.HttpConnection.Models
{
    /// <summary>
    /// Readonly interface to <see cref="ObjectivesFilter"/>.
    /// </summary>
    public interface IReadonlyObjectivesFilter
    {
        /// <summary>
        /// Collection of allowed objective types.
        /// </summary>
        IReadOnlyList<ID<ObjectiveType>> TypeIds { get; }

        /// <summary>
        /// Collection of allowed statuses.
        /// </summary>
        IReadOnlyList<int> Statuses { get; }

        /// <summary>
        /// Filter out objectives with parent defined by this ID.
        /// </summary>
        int? ExceptChildrenOf { get; }

        /// <summary>
        /// Get objectives that attached to this BIM element.
        /// </summary>
        string BimElementGuid { get; }

        /// <summary>
        /// Objective name filter.
        /// </summary>
        string TitlePart { get; }

        /// <summary>
        /// Upper limit of objective's creation date.
        /// </summary>
        DateTime? CreatedBefore { get; }

        /// <summary>
        /// Lower limit of objective's creation date.
        /// </summary>
        DateTime? CreatedAfter { get; }

        /// <summary>
        /// Upper limit of objective's updated date.
        /// </summary>
        DateTime? UpdatedBefore { get; }

        /// <summary>
        /// Lower limit of objective's updated date.
        /// </summary>
        DateTime? UpdatedAfter { get; }

        /// <summary>
        /// Upper limit of objective's due date.
        /// </summary>
        DateTime? FinishedBefore { get; }

        /// <summary>
        /// Lower limit of objective's due date.
        /// </summary>
        DateTime? FinishedAfter { get; }

        /// <summary>
        /// Collection of required dynamic fields id with values.
        /// </summary>
        IReadOnlyList<ObjectiveFilterParameters.DynamicFieldFilterValue> DynamicFieldValues { get; }

        /// <summary>
        /// Get objectives where bim elements with this ParentName.
        /// </summary>
        string BimElementParent { get; }
    }
}
