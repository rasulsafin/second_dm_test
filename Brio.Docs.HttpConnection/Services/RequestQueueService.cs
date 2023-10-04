using System.Threading.Tasks;
using Brio.Docs.Client;
using Brio.Docs.Client.Services;

namespace Brio.Docs.HttpConnection.Services
{
    internal class RequestQueueService : ServiceBase, IRequestQueueService
    {
        private static readonly string PATH = "RequestQueue";

        public RequestQueueService(Connection connection)
            : base(connection)
        {
        }

        public async Task Cancel(string id) => await Connection.GetDataAsync<bool>($"{PATH}/cancel/{{0}}", id);

        public async Task<double> GetProgress(string id) => await Connection.GetDataAsync<double>($"{PATH}/{{0}}", id);

        public async Task<RequestResult> GetResult(string id) => await Connection.GetDataAsync<RequestResult>($"{PATH}/result/{{0}}", id);
    }
}
