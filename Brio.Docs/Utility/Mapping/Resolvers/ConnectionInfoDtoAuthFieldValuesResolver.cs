using System.Collections.Generic;
using AutoMapper;
using Brio.Docs.Common.Dtos;
using Brio.Docs.Database.Models;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Utility.Mapping.Resolvers
{
    public class ConnectionInfoDtoAuthFieldValuesResolver : IValueResolver<IConnectionInfoDto, ConnectionInfo, ICollection<AuthFieldValue>>
    {
        private readonly CryptographyHelper helper;
        private readonly ILogger<ConnectionInfoDtoAuthFieldValuesResolver> logger;

        public ConnectionInfoDtoAuthFieldValuesResolver(
            CryptographyHelper helper,
            ILogger<ConnectionInfoDtoAuthFieldValuesResolver> logger)
        {
            this.helper = helper;
            this.logger = logger;
            logger.LogTrace("ConnectionInfoDtoAuthFieldValuesResolver created");
        }

        public ICollection<AuthFieldValue> Resolve(IConnectionInfoDto source, ConnectionInfo destination, ICollection<AuthFieldValue> destMember, ResolutionContext context)
        {
            logger.LogTrace("Resolve started");
            var list = new List<AuthFieldValue>();
            foreach (var property in source.AuthFieldValues)
            {
                list.Add(new AuthFieldValue()
                {
                    Key = property.Key,
                    Value = helper.EncryptAes(property.Value),
                });
            }

            logger.LogDebug("Created list: {@List}", list);
            return list;
        }
    }
}
