using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Brio.Docs.Common.Dtos;
using Brio.Docs.Database.Models;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Utility.Mapping.Resolvers
{
    public class ConnectionTypeDtoAppPropertiesResolver : IValueResolver<IConnectionTypeDto, ConnectionType, IEnumerable<AppProperty>>
    {
        private readonly CryptographyHelper helper;
        private readonly ILogger<ConnectionTypeDtoAppPropertiesResolver> logger;

        public ConnectionTypeDtoAppPropertiesResolver(
            CryptographyHelper helper,
            ILogger<ConnectionTypeDtoAppPropertiesResolver> logger)
        {
            this.helper = helper;
            this.logger = logger;
            logger.LogTrace("ConnectionTypeDtoAppPropertiesResolver created");
        }

        public IEnumerable<AppProperty> Resolve(IConnectionTypeDto source, ConnectionType destination, IEnumerable<AppProperty> destMember, ResolutionContext context)
        {
            logger.LogTrace("Resolve started");

            var list = source.AppProperties?.Select(
                    property => new AppProperty
                    {
                        Key = property.Key,
                        Value = helper.EncryptAes(property.Value),
                    })
               .ToList();

            logger.LogDebug("Created list: {@List}", list);
            return list;
        }
    }
}
