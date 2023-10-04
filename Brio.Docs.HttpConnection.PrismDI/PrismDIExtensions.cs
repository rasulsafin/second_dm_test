using System;
using System.Net.Http;
using Brio.Docs.Client.Services;
using Brio.Docs.HttpConnection.Services;
using Microsoft.Extensions.Logging;
using Prism.Ioc;

namespace Brio.Docs.HttpConnection
{
    public static class PrismDIExtensions
    {
        public static void AddDocumentManagement(this IContainerRegistry container, Func<IContainerProvider, HttpClient> clientResolver)
        {
            container.RegisterSingleton<Connection>(x => new Connection(clientResolver(x), x.Resolve<ILogger<Connection>>()));

            container.RegisterSingleton<AuthenticationService>();
            container.RegisterSingleton<IConnectionService, ConnectionService>();
            container.RegisterSingleton<ISynchronizationService, SynchronizationService>();
            container.RegisterSingleton<IAuthorizationService, AuthorizationService>();
            container.RegisterSingleton<IItemService, ItemService>();
            container.RegisterSingleton<IObjectiveTypeService, ObjectiveTypeService>();
            container.RegisterSingleton<IObjectiveService, ObjectiveService>();
            container.RegisterSingleton<IReportService, ReportService>();
            container.RegisterSingleton<IProjectService, ProjectService>();
            container.RegisterSingleton<IUserService, UserService>();
            container.RegisterSingleton<IRequestQueueService, RequestQueueService>();
            container.RegisterSingleton<IBimElementService, BimElementService>();
        }
    }
}
