using Brio.Docs.Client;
using Brio.Docs.Common.Dtos;

namespace Brio.Docs.HttpConnection.Models
{
    /// <summary>
    /// Interface for Dynamic Values in Objective.
    /// </summary>
    public interface IDynamicField
    {
        /// <summary>
        /// ID of the object.
        /// </summary>
        ID<IDynamicField> ID { get; set; }

        /// <summary>
        /// ID of the object.
        /// </summary>
        string Key { get; set; }

        /// <summary>
        /// Name to be displayed.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Type for current implementation of DynamicField.
        /// </summary>
        DynamicFieldType Type { get; }

        /// <summary>
        /// Value of the dynamic field.
        /// </summary>
        object Value { get; set; }
    }
}
