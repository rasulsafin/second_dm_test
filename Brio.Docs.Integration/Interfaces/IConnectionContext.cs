using Brio.Docs.Integration.Dtos;

namespace Brio.Docs.Integration.Interfaces
{
    /// <summary>
    /// Context of the working with external connection.
    /// </summary>
    public interface IConnectionContext
    {
        /// <summary>
        /// Gets synchronizer for projects.
        /// </summary>
        ISynchronizer<ProjectExternalDto> ProjectsSynchronizer { get; }

        /// <summary>
        /// Gets synchronizer for objectives.
        /// </summary>
        ISynchronizer<ObjectiveExternalDto> ObjectivesSynchronizer { get; }
    }
}
