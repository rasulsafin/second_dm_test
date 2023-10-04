using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Brio.Docs.Database.Models;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Interfaces;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Synchronization.Utils
{
    internal class MapperHelper
        : IConverter<IReadOnlyCollection<ObjectiveExternalDto>, IReadOnlyCollection<Objective>>,
            IConverter<IReadOnlyCollection<ProjectExternalDto>, IReadOnlyCollection<Project>>
    {
        private readonly ILogger<MapperHelper> logger;
        private readonly IMapper mapper;

        public MapperHelper(ILogger<MapperHelper> logger, IMapper mapper)
        {
            this.logger = logger;
            this.mapper = mapper;
        }

        public Task<IReadOnlyCollection<Objective>> Convert(IReadOnlyCollection<ObjectiveExternalDto> externalDtos)
        {
            logger.LogTrace("Map started");
            var objectives = mapper.Map<IReadOnlyCollection<Objective>>(externalDtos);

            foreach (var objective in objectives)
            {
                var external = externalDtos.FirstOrDefault(x => x.ExternalID == objective.ExternalID);

                if (!string.IsNullOrEmpty(external?.ParentObjectiveExternalID))
                {
                    objective.ParentObjective =
                        objectives.FirstOrDefault(x => x.ExternalID == external.ParentObjectiveExternalID);
                }
            }

            return Task.FromResult(objectives);
        }

        public Task<IReadOnlyCollection<Project>> Convert(IReadOnlyCollection<ProjectExternalDto> externalDtos)
        {
            logger.LogTrace("Map started for externalDtos: {@Dtos}", externalDtos);
            var result = mapper.Map<IReadOnlyCollection<Project>>(externalDtos);
            return Task.FromResult(result);
        }
    }
}
