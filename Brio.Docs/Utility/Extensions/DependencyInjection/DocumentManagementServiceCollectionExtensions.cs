using System;
using AutoMapper;
using Brio.Docs.Client.Services;
using Brio.Docs.Client.Services.ForApi;
using Brio.Docs.Client.Services.ForApi.Helpers;
using Brio.Docs.Database;
using Brio.Docs.Integration;
using Brio.Docs.Integration.Factories;
using Brio.Docs.Integration.Interfaces;
using Brio.Docs.Interfaces;
using Brio.Docs.Reports;
using Brio.Docs.Services;
using Brio.Docs.Services.ForApi;
using Brio.Docs.Services.ForApi.Helpers;
using Brio.Docs.Synchronization.Interfaces;
using Brio.Docs.Utility;
using Brio.Docs.Utility.Factories;
using Brio.Docs.Utility.Mapping.Converters;
using Brio.Docs.Utility.Mapping.Resolvers;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DocumentManagementServiceCollectionExtensions
    {
        public static IServiceCollection AddDocumentManagement(this IServiceCollection services)
        {
            services.AddMappingResolvers();

            services.AddScoped<IAuthorizationService, AuthorizationService>();
            services.AddScoped<IConnectionService, ConnectionService>();
            services.AddScoped<ISynchronizationService, SynchronizationService>();
            services.AddScoped<IItemService, ItemService>();
            services.AddScoped<IObjectiveService, ObjectiveService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IObjectiveTypeService, ObjectiveTypeService>();
            services.AddScoped<IProjectService, ProjectService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IConnectionTypeService, ConnectionTypeService>();
            services.AddScoped<IBimElementService, BimElementService>();
            
            // ForApi
            services.AddScoped<ISynchronizationForApiService, SynchronizationForApiService>();
            services.AddScoped<IProjectForApiService, ProjectForApiService>();
            services.AddScoped<IAuthorizationForApiService, AuthorizationFroApiService>();
            services.AddScoped<IUserForApiService, UserForApiService>();
            services.AddScoped<IObjectiveForApiService, ObjectiveForApiService>();
            services.AddScoped<IObjectiveTypeForApiService, ObjectiveTypeForApiService>();
            services.AddScoped<IItemForApiService, ItemForApiService>();
            services.AddScoped<IConnectionForApiService, ConnectionForApiService>();
            services.AddScoped<IRequestForApiService, RequestQueueForApiService>();
            services.AddScoped<IRequestQueueForApiService, RequestQueueForApiService>();
            
            
            services.AddSingleton<IHttpRequestForApiHandlerService, HttpRequestForApiHandlerService>();

            services.AddSingleton<IRequestService, RequestQueueService>();
            services.AddSingleton<IRequestQueueService, RequestQueueService>();
            services.AddSingleton<CryptographyHelper>();
            services.AddSingleton<ReportGenerator>();

            services.AddFactories();
            services.AddSynchronizer();
            services.AddExternal();
            services.AddHelpers();

            return services;
        }

        public static IServiceCollection AddHelpers(this IServiceCollection services)
        {
            services.AddScoped<ItemsHelper>();
            services.AddScoped<DynamicFieldsHelper>();
            services.AddScoped<BimElementsHelper>();
            services.AddScoped<ConnectionHelper>();

            return services;
        }

        public static IServiceCollection AddMappingResolvers(this IServiceCollection services)
        {
            services.AddTransient<ConnectionTypeAppPropertiesResolver>();
            services.AddTransient<ConnectionTypeDtoAppPropertiesResolver>();
            services.AddTransient<ConnectionInfoAuthFieldValuesResolver>();
            services.AddTransient<ConnectionInfoDtoAuthFieldValuesResolver>();

            services.AddTransient<DynamicFieldDtoToModelValueResolver>();
            services.AddTransient<DynamicFieldExternalToModelValueResolver>();
            services.AddTransient<DynamicFieldModelToDtoValueResolver>();
            services.AddTransient<DynamicFieldModelToExternalValueResolver>();

            services.AddTransient<ObjectiveExternalDtoProjectIdResolver>();
            services.AddTransient<ObjectiveExternalDtoObjectiveTypeResolver>();
            services.AddTransient<ObjectiveExternalDtoObjectiveTypeIdResolver>();
            services.AddTransient<ObjectiveExternalDtoAuthorIdResolver>();
            services.AddTransient<ObjectiveObjectiveTypeResolver>();
            services.AddTransient<ObjectiveProjectIDResolver>();

            services.AddTransient<BimElementObjectiveTypeConverter>();
            services.AddTransient<DynamicFieldModelToDtoConverter>();

            services.AddTransient<ItemProjectDirectoryResolver>();

            return services;
        }

        private static IServiceCollection AddFactories(this IServiceCollection services)
        {
            services.AddScoped<Func<Type, IConnection>>(x => type => (IConnection)x.GetRequiredService(type));
            services.AddScoped<IFactory<Type, IConnection>, Factory<Type, IConnection>>();

            services.AddScoped<Func<IServiceScope, Type, IConnection>>(
                x => (scope, type) => (IConnection)(scope?.ServiceProvider ?? x).GetRequiredService(type));
            services.AddScoped<IFactory<IServiceScope, Type, IConnection>, Factory<IServiceScope, Type, IConnection>>();

            services.AddScoped<IFactory<IServiceScope, ConnectionHelper>, ConnectionHelperFactory>();

            services.AddScopedFactory<DMContext>();
            services.AddScopedFactory<IMapper>();
            services.AddScopedFactory<ISynchronizer>();
            return services;
        }

        private static IServiceCollection AddExternal(this IServiceCollection services)
        {
            foreach (var action in ConnectionCreator.GetDependencyInjectionMethods())
                action?.Invoke(services);
            return services;
        }
    }
}
