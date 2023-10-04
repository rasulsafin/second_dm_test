using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Brio.Docs.Integration.Extensions
{
    public static class LoggerExtensions
    {
        public static IDisposable BeginMethodScope(this ILogger logger, [CallerMemberName] string method = "")
            => new ScopeContainer(logger.BeginScope("{@MethodName}", method), () => logger.LogTrace("Method ends"));

        private class ScopeContainer : IDisposable
        {
            private readonly IDisposable scope;
            private readonly Action actionOnDispose;

            public ScopeContainer(IDisposable scope, Action actionOnDispose)
            {
                this.scope = scope;
                this.actionOnDispose = actionOnDispose;
            }

            public void Dispose()
            {
                actionOnDispose();
                scope?.Dispose();
            }
        }
    }
}
