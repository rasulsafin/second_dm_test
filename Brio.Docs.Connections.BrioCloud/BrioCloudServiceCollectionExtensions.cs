using Brio.Docs.Connections.BrioCloud;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class BrioCloudServiceCollectionExtensions
    {
        public static IServiceCollection AddBrioCloud(this IServiceCollection services)
        {
            services.AddSingleton<BrioCloudConnection>();

            return services;
        }
    }
}
