using System;
using System.Threading;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Interfaces;

namespace Brio.Docs.Integration
{
    public abstract class AConnectionContext : IConnectionContext
    {
        private readonly Lazy<ISynchronizer<ProjectExternalDto>> projectsSynchronizer;
        private readonly Lazy<ISynchronizer<ObjectiveExternalDto>> objectivesSynchronizer;

        protected AConnectionContext()
        {
            projectsSynchronizer = new Lazy<ISynchronizer<ProjectExternalDto>>(
                    CreateProjectsSynchronizer,
                    LazyThreadSafetyMode.ExecutionAndPublication);
            objectivesSynchronizer = new Lazy<ISynchronizer<ObjectiveExternalDto>>(
                    CreateObjectivesSynchronizer,
                    LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public ISynchronizer<ProjectExternalDto> ProjectsSynchronizer
            => projectsSynchronizer.Value;

        public ISynchronizer<ObjectiveExternalDto> ObjectivesSynchronizer
            => objectivesSynchronizer.Value;

        protected abstract ISynchronizer<ProjectExternalDto> CreateProjectsSynchronizer();

        protected abstract ISynchronizer<ObjectiveExternalDto> CreateObjectivesSynchronizer();
    }
}
