using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Database.Models;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Utility.Mapping.Converters
{
    public class DynamicFieldModelToDtoConverter : IValueConverter<ICollection<DynamicField>, ICollection<DynamicFieldDto>>
    {
        private readonly DynamicFieldsHelper dynamicFieldHelper;
        private readonly ILogger<DynamicFieldModelToDtoConverter> logger;

        public DynamicFieldModelToDtoConverter(DynamicFieldsHelper dynamicFieldHelper, ILogger<DynamicFieldModelToDtoConverter> logger)
        {
            this.dynamicFieldHelper = dynamicFieldHelper;
            this.logger = logger;
            logger.LogTrace("DynamicFieldModelToDtoConverter created");
        }

        public ICollection<DynamicFieldDto> Convert(ICollection<DynamicField> sourceMember, ResolutionContext context)
        {
            logger.LogTrace("Convert started");
            var list = new List<DynamicFieldDto>();
            foreach (var field in sourceMember)
            {
                var dynamicFieldDto = Task.Run(async () => await dynamicFieldHelper.BuildObjectDynamicField(field)).Result;
                list.Add(dynamicFieldDto);
            }

            logger.LogTrace("Created list with fields: {@List}", list);
            return list;
        }
    }
}
