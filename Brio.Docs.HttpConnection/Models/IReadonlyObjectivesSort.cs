using System.Collections.Generic;

namespace Brio.Docs.HttpConnection.Models
{
    /// <summary>
    /// Readonly interface to <see cref="ObjectivesSort"/>.
    /// </summary>
    public interface IReadonlyObjectivesSort
    {
        /// <summary>
        /// Collection of sorts to be applied.
        /// </summary>
        IReadOnlyList<ISortParameter> Sorts { get; }
    }
}
