using System;
using System.Collections.Generic;
using Brio.Docs.Integration.Dtos;
using Brio.Docs.Integration.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Brio.Docs.Integration.Interfaces
{
    /// <summary>
    /// The interface for register external connection in the system.
    /// </summary>
    public interface IConnectionMeta
    {
        /// <summary>
        /// Get the information about the current Connection.
        /// </summary>
        /// <returns>Filed ConnectionTypeDto.</returns>
        ConnectionTypeExternalDto GetConnectionTypeInfo();

        /// <summary>
        /// Gets type of class that implements IConnection.
        /// </summary>
        /// <returns>The type of the class that implements IConnection interface.</returns>
        Type GetIConnectionType();

        /// <summary>
        /// Gets action which adds needed classes to the IoC container.
        /// </summary>
        /// <returns>The action for injecting classes.</returns>
        Action<IServiceCollection> AddToDependencyInjectionMethod();

        /// <summary>
        /// Gets properties needed to ignore by logging.
        /// </summary>
        /// <returns>The enumeration of properties needed to ignore by logging.</returns>
        IEnumerable<GettingPropertyExpression> GetPropertiesForIgnoringByLogging();
    }
}
