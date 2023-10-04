using System;
using AutoMapper;
using Brio.Docs.Database;
using Brio.Docs.Integration.Factories;
using Brio.Docs.Integration.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Utility.Factories
{
    public class ConnectionHelperFactory : IFactory<IServiceScope, ConnectionHelper>
    {
        private readonly IFactory<IServiceScope, DMContext> contextFactory;
        private readonly IFactory<IServiceScope, IMapper> mapperFactory;
        private readonly IFactory<IServiceScope, Type, IConnection> connectionFactory;
        private readonly ILoggerFactory loggerFactory;

        public ConnectionHelperFactory(
            IFactory<IServiceScope, DMContext> contextFactory,
            IFactory<IServiceScope, IMapper> mapperFactory,
            IFactory<IServiceScope, Type, IConnection> connectionFactory,
            ILoggerFactory loggerFactory)
        {
            this.contextFactory = contextFactory;
            this.mapperFactory = mapperFactory;
            this.connectionFactory = connectionFactory;
            this.loggerFactory = loggerFactory;
        }

        public ConnectionHelper Create(IServiceScope scope)
        {
            return new ConnectionHelper(
                contextFactory.Create(scope),
                mapperFactory.Create(scope),
                new Factory<Type, IConnection>(x => connectionFactory.Create(scope, x)),
                loggerFactory.CreateLogger<ConnectionHelper>());
        }
    }
}
