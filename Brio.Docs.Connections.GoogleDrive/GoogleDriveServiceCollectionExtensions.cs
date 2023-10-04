using Brio.Docs.Connections.GoogleDrive;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class GoogleDriveServiceCollectionExtensions
    {
        public static IServiceCollection AddGoogleDrive(this IServiceCollection services)
        {
            services.AddScoped<GoogleConnection>();
            return services;
        }
    }
}
