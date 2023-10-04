using System;
using System.Linq;
using AutoMapper;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Common.Dtos;
using Brio.Docs.Database;
using Brio.Docs.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Utility.Mapping.Resolvers
{
    public class DynamicFieldModelToDtoValueResolver : IValueResolver<IDynamicField, DynamicFieldDto, object>
    {
        private readonly DMContext dbContext;
        private readonly IMapper mapper;
        private readonly ILogger<DynamicFieldModelToDtoValueResolver> logger;

        public DynamicFieldModelToDtoValueResolver(DMContext dbContext, IMapper mapper, ILogger<DynamicFieldModelToDtoValueResolver> logger)
        {
            this.dbContext = dbContext;
            this.mapper = mapper;
            this.logger = logger;
            logger.LogTrace("DynamicFieldModelToDtoValueResolver created");
        }

        public object Resolve(IDynamicField source, DynamicFieldDto destination, object destMember, ResolutionContext context)
        {
            logger.LogTrace("Resolve started with source: {@Source} & destination {@Destination}", source, destination);
            var type = (DynamicFieldType)Enum.Parse(typeof(DynamicFieldType), source.Type);

            return type switch
            {
                DynamicFieldType.OBJECT => null,
                DynamicFieldType.STRING => source.Value,
                DynamicFieldType.BOOL => bool.Parse(source.Value),
                DynamicFieldType.INTEGER => int.Parse(source.Value),
                DynamicFieldType.FLOAT => float.Parse(source.Value),
                DynamicFieldType.ENUM => GetEnum(source.Value, source.ConnectionInfoID),
                DynamicFieldType.DATE => DateTime.Parse(source.Value),
                _ => null,
            };
        }

        private Enumeration GetEnum(string valueFromDb, int? id)
        {
            logger.LogTrace("GetEnum started with valueFromDb: {@ValueFromDb}", valueFromDb);
            var enumValue = dbContext.EnumerationValues
                .AsNoTracking()
                .FirstOrDefault(x => x.ID == int.Parse(valueFromDb));

            logger.LogDebug("Found enum value: {@EnumValue}", enumValue);

            var enumType = dbContext.EnumerationTypes
                .AsNoTracking()
                .FirstOrDefault(x => x.ID == enumValue.EnumerationTypeID);

            IQueryable<EnumerationValue> enumValuesFromDb;
            if (id != 0)
            {
                enumValuesFromDb = dbContext.ConnectionInfoEnumerationValues
                .AsNoTracking()
                .Where(x => x.ConnectionInfoID == id)
                .Include(x => x.EnumerationValue)
                .Select(x => x.EnumerationValue)
                .Where(x => x.EnumerationTypeID == enumType.ID);
            }
            else
            {
                enumValuesFromDb = dbContext.EnumerationValues
                    .AsNoTracking()
                    .Where(x => x.EnumerationTypeID == enumType.ID);
            }

            enumType.EnumerationValues = enumValuesFromDb.ToList();

            var type = mapper.Map<EnumerationTypeDto>(enumType);
            logger.LogDebug("Mapped type: {@Type}", type);
            var value = mapper.Map<EnumerationValueDto>(enumValue);
            logger.LogDebug("Mapped value: {@Value}", value);

            return new Enumeration() { EnumerationType = type, Value = value, };
        }
    }
}
