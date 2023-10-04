using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Brio.Docs.Database.Models;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Interfaces;
using Brio.Docs.Integration.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Brio.Docs.Integration
{
    public static class ConnectionCreator
    {
        private static readonly string SEARCH_PATTERN = "*Brio.Docs.Connections.*.dll";

        private static readonly Lazy<IReadOnlyCollection<IConnectionMeta>> CONNECTION_METAS = new (
            GetAllConnectionMetas,
            LazyThreadSafetyMode.PublicationOnly);

        public static IEnumerable<Action<IServiceCollection>> GetDependencyInjectionMethods()
            => CONNECTION_METAS.Value.Select(x => x.AddToDependencyInjectionMethod());

        public static IEnumerable<GettingPropertyExpression> GetPropertiesForIgnoringByLogging()
            => CONNECTION_METAS.Value.SelectMany(x => x.GetPropertiesForIgnoringByLogging());

        public static Type GetConnection(ConnectionType connectionType)
            => CONNECTION_METAS.Value.FirstOrDefault(x => x.GetConnectionTypeInfo().Name == connectionType.Name)
              ?.GetIConnectionType();

        public static IEnumerable<ConnectionTypeExternalDto> GetAllConnectionTypes()
            => CONNECTION_METAS.Value.Select(x => x.GetConnectionTypeInfo());

        private static IReadOnlyCollection<IConnectionMeta> GetAllConnectionMetas()
            => Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, SEARCH_PATTERN)
               .Select(x => Assembly.Load(AssemblyName.GetAssemblyName(x)))
               .SelectMany(x => x.GetTypes())
               .Where(x => typeof(IConnectionMeta).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
               .Select(Activator.CreateInstance)
               .Cast<IConnectionMeta>()
               .ToList()
               .AsReadOnly();
    }
}
