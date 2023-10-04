using System.Threading;
using System.Threading.Tasks;
using Brio.Docs.Common.Dtos;
using Brio.Docs.Integration.Dtos;

namespace Brio.Docs.Integration.Interfaces
{
    /// <summary>
    /// Interface for any type of connection.
    /// </summary>
    public interface IConnection
    {
        /// <summary>
        /// Connect to remote DM.
        /// </summary>
        /// <param name="info">Information about the connection.</param>
        /// <param name="token">Token to cancel connection.</param>
        /// <returns>Result success and additional result data.</returns>
        Task<ConnectionStatusDto> Connect(ConnectionInfoExternalDto info, CancellationToken token);

        /// <summary>
        /// Current status of the connection.
        /// </summary>
        /// <param name="info">Information about the connection.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<ConnectionStatusDto> GetStatus(ConnectionInfoExternalDto info);

        /// <summary>
        /// Method fills all the relevant fields in ConnectionInfo
        /// e.g. AuthFieldValues and EnumerationTypes in order to link them to User.
        /// </summary>
        /// <param name="info">ConnectionInfoDto to fill in.</param>
        /// <returns>Filed ConnectionInfoDto.</returns>
        Task<ConnectionInfoExternalDto> UpdateConnectionInfo(ConnectionInfoExternalDto info);

        /// <summary>
        /// Get the context for working with this connection.
        /// </summary>
        /// <param name="info">ConnectionInfoDto to fill in.</param>
        /// <returns>The context from the connection for a synchronization.</returns>
        Task<IConnectionContext> GetContext(ConnectionInfoExternalDto info);

        /// <summary>
        /// Get the wrapper for working with this connection's storage.
        /// </summary>
        /// <param name="info">ConnectionInfoDto to fill in.</param>
        /// <returns>The storage for working with files.</returns>
        Task<IConnectionStorage> GetStorage(ConnectionInfoExternalDto info);
    }
}
