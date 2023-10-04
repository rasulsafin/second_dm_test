using System;
using System.Collections.Generic;
using AutoMapper;
using Brio.Docs.Common.Dtos;
using Brio.Docs.Database.Models;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Utility.Mapping.Resolvers
{
    public class ConnectionInfoAuthFieldValuesResolver : IValueResolver<ConnectionInfo, IConnectionInfoDto, IDictionary<string, string>>
    {
        private readonly CryptographyHelper helper;
        private readonly ILogger<ConnectionInfoAuthFieldValuesResolver> logger;

        public ConnectionInfoAuthFieldValuesResolver(
            CryptographyHelper helper,
            ILogger<ConnectionInfoAuthFieldValuesResolver> logger)
        {
            this.helper = helper;
            this.logger = logger;
            logger.LogTrace("ConnectionInfoAuthFieldValuesResolver created");
        }

        public IDictionary<string, string> Resolve(ConnectionInfo source, IConnectionInfoDto destination, IDictionary<string, string> destMember, ResolutionContext context)
        {
            logger.LogTrace("Resolve started");
            var dictionary = new Dictionary<string, string>();
            foreach (var property in source.AuthFieldValues ?? ArraySegment<AuthFieldValue>.Empty)
                dictionary.Add(property.Key, helper.DecryptAes(property.Value));

            logger.LogDebug("Created dictionary with keys: {@Keys}", dictionary.Keys);
            return dictionary;
        }
    }
}
