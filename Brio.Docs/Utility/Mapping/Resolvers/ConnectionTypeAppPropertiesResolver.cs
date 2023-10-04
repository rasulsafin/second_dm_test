using System.Collections.Generic;
using AutoMapper;
using Brio.Docs.Common.Dtos;
using Brio.Docs.Database.Models;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Utility.Mapping.Resolvers
{
    public class ConnectionTypeAppPropertiesResolver : IValueResolver<ConnectionType, IConnectionTypeDto, IDictionary<string, string>>
    {
        private readonly CryptographyHelper helper;
        private readonly ILogger<ConnectionTypeAppPropertiesResolver> logger;

        public ConnectionTypeAppPropertiesResolver(CryptographyHelper helper, ILogger<ConnectionTypeAppPropertiesResolver> logger)
        {
            this.helper = helper;
            this.logger = logger;
            logger.LogTrace("ConnectionTypeAppPropertiesResolver created");
        }

        public IDictionary<string, string> Resolve(ConnectionType source, IConnectionTypeDto destination, IDictionary<string, string> destMember, ResolutionContext context)
        {
            logger.LogTrace("Resolve started");
            var dictionary = new Dictionary<string, string>();
            foreach (var property in source.AppProperties)
            {
                dictionary.Add(property.Key, helper.DecryptAes(property.Value));
            }

            logger.LogDebug("Created dictionary with keys: {@Keys}", dictionary.Keys);
            return dictionary;
        }
    }
}
