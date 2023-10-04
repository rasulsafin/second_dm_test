using System.Collections.Generic;
using Brio.Docs.Database.Models;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Interfaces;
using Brio.Docs.Synchronization;
using Brio.Docs.Synchronization.Interfaces;
using Brio.Docs.Synchronization.Mergers;
using Brio.Docs.Synchronization.Mergers.ChildrenMergers;
using Brio.Docs.Synchronization.Strategies;
using Brio.Docs.Synchronization.Utilities.Finders;
using Brio.Docs.Synchronization.Utils;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SynchronizationServiceCollectionExtensions
    {
        public static IServiceCollection AddSynchronizer(this IServiceCollection services)
        {
            services.AddScoped<Synchronizer>();
            services.AddScoped<ISynchronizer, Synchronizer>(x => x.GetService<Synchronizer>());
            services.AddScoped<ISynchronizerProcessor, SynchronizerProcessor>();
            services.AddScoped<StrategyHelper>();
            services.AddScoped<ISynchronizationStrategy<Project>, ProjectStrategy>();
            services.AddScoped<ISynchronizationStrategy<Objective>, ObjectiveStrategy>();

            services.AddScoped<MapperHelper>();
            services.AddScoped<IConverter<
                IReadOnlyCollection<ProjectExternalDto>,
                IReadOnlyCollection<Project>>>(provider => provider.GetService<MapperHelper>());
            services.AddScoped<IConverter<
                IReadOnlyCollection<ObjectiveExternalDto>,
                IReadOnlyCollection<Objective>>>(provider => provider.GetService<MapperHelper>());

            services.AddScoped<IMerger<Project>, ProjectMerger>();
            services.AddScoped<IMerger<Objective>, ObjectiveMerger>();
            services.AddScoped<IMerger<Item>, ItemMerger>();
            services.AddScoped<IMerger<DynamicField>, DynamicFieldMerger>();
            services.AddScoped<IMerger<Location>, LocationMerger>();
            services.AddScoped<IMerger<BimElement>, BimElementMerger>();

            services.AddScoped<IAttacher<Project>, ProjectAttacher>();
            services.AddScoped<IAttacher<Objective>, ObjectiveAttacher>();
            services.AddScoped<IAttacher<Item>, ItemAttacher>();
            services.AddScoped<IAttacher<BimElement>, BimElementAttacher>();

            services.AddScoped<IExternalIdUpdater<Item>, ItemExternalIdUpdater>();
            services.AddScoped<IExternalIdUpdater<DynamicField>, DynamicFieldExternalIdUpdater>();

            services.AddProjectChildrenMergers();
            services.AddObjectiveChildrenMergers();
            services.AddDynamicFieldChildrenMergers();

            return services;
        }

        private static IServiceCollection AddDynamicFieldChildrenMergers(this IServiceCollection services)
            => services
               .AddScoped<IChildrenMerger<DynamicField, DynamicField>, DynamicFieldDynamicFieldsMerger>()
               .AddFactory<IChildrenMerger<DynamicField, DynamicField>>();

        private static IServiceCollection AddObjectiveChildrenMergers(this IServiceCollection services)
        {
            services.AddScoped<IChildrenMerger<Objective, Item>, ObjectiveItemsMerger>();

            services.AddScoped<IChildrenMerger<Objective, DynamicField>, ObjectiveDynamicFieldsMerger>();
            services.AddScoped<IChildrenMerger<Objective, BimElement>, ObjectiveBimElementsMerger>();

            services.AddFactory<IChildrenMerger<Objective, Item>>();
            services.AddFactory<IChildrenMerger<Objective, DynamicField>>();
            services.AddFactory<IChildrenMerger<Objective, BimElement>>();
            return services;
        }

        private static IServiceCollection AddProjectChildrenMergers(this IServiceCollection services)
            => services
               .AddScoped<IChildrenMerger<Project, Item>, ProjectItemsMerger>()
               .AddFactory<IChildrenMerger<Project, Item>>();
    }
}
