using System;
using Brio.Docs.Integration.Factories;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class FactoryServiceCollectionExtensions
    {
        public static IServiceCollection AddScopedFactory<TResult>(this IServiceCollection services)
        {
            services.AddScoped<Func<IServiceScope, TResult>>(
                x => scope => (scope?.ServiceProvider ?? x).GetRequiredService<TResult>());
            services.AddScoped<IFactory<IServiceScope, TResult>, Factory<IServiceScope, TResult>>();
            return services;
        }

        public static IServiceCollection AddFactory<TResult>(this IServiceCollection services)
        {
            services.AddScoped<Func<TResult>>(x => x.GetRequiredService<TResult>);
            services.AddScoped<IFactory<TResult>, Factory<TResult>>();
            return services;
        }
    }
}
