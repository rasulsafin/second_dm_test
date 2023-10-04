using AutoMapper;
using Brio.Docs.Client.Dtos;
using Brio.Docs.Common.Dtos;
using Brio.Docs.Database.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Brio.Docs.Utility.Mapping.Resolvers
{
    public class DynamicFieldDtoToModelValueResolver : IValueResolver<DynamicFieldDto, IDynamicField, string>
    {
        private readonly ILogger<DynamicFieldDtoToModelValueResolver> logger;

        public DynamicFieldDtoToModelValueResolver(ILogger<DynamicFieldDtoToModelValueResolver> logger)
        {
            this.logger = logger;
            logger.LogTrace("DynamicFieldDtoToModelValueResolver created");
        }

        public string Resolve(DynamicFieldDto source, IDynamicField destination, string destMember, ResolutionContext context)
        {
            logger.LogTrace("Resolve started with source: {@Source} & destination {@Destination}", source, destination);
            if (source.Type == DynamicFieldType.ENUM)
            {
                switch (source.Value)
                {
                    case JObject value:
                        return value.ToObject<Enumeration>().Value.ID.ToString();
                    case Enumeration value:
                        return value.Value.ID.ToString();
                    default:
                        return null;
                }
            }

            if (source.Type == DynamicFieldType.OBJECT)
                return null;

            return source.Value.ToString();
        }
    }
}
