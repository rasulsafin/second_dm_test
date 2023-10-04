using Brio.Docs.Connections.YandexDisk;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class YandexDiskServiceCollectionExtensions
    {
        public static IServiceCollection AddYandexDisk(this IServiceCollection services)
        {
            services.AddScoped<YandexConnection>();
            return services;
        }
    }
}
