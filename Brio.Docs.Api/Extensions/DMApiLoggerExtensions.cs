using Brio.Docs.Utility.Extensions;
using Serilog;

namespace Brio.Docs.Api.Extensions
{
    public static class DMApiLoggerExtensions
    {
        public static LoggerConfiguration DestructureByIgnoringSensitive(this LoggerConfiguration configuration)
        {
            return configuration.DestructureByIgnoringSensitiveDMInfo();
        }
    }
}
