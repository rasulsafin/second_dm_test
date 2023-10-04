using System;
using System.Linq;
using Brio.Docs.Database.Models;
using Brio.Docs.Integration;
using Brio.Docs.Integration.Dtos;
using Destructurama;
using Serilog;

namespace Brio.Docs.Utility.Extensions
{
    public static class DMLoggerExtensions
    {
        public static LoggerConfiguration DestructureByIgnoringSensitiveDMInfo(this LoggerConfiguration configuration)
            => configuration.Destructure.ByIgnoringProperties<User>(x => x.PasswordHash, x => x.PasswordSalt)
               .Destructure.ByIgnoringProperties<AuthFieldValue>(x => x.Value)
               .Destructure.ByIgnoringProperties<ConnectionTypeExternalDto>(x => x.AppProperties)
               .Destructure.ByIgnoringProperties<ConnectionInfoExternalDto>(x => x.AuthFieldValues)
               .DestructureByIgnoringSensitiveExternalInfo();

        private static LoggerConfiguration DestructureByIgnoringSensitiveExternalInfo(
            this LoggerConfiguration configuration)
        {
            var type = typeof(LoggerConfigurationIgnoreExtensions);
            var method = type.GetMethod(nameof(LoggerConfigurationIgnoreExtensions.ByIgnoringProperties));

            foreach (var expression in ConnectionCreator.GetPropertiesForIgnoringByLogging()
               .ToLookup(x => x.SourceType))
            {
                var generic = method!.MakeGenericMethod(expression.Key);
                var arrayBase = expression.Select(x => x.Expression).ToArray();
                var array = Array.CreateInstance(arrayBase[0].GetType(), arrayBase.Length);
                for (int i = 0; i < array.Length; i++)
                    array.SetValue(arrayBase[i], i);
                configuration = (LoggerConfiguration)generic.Invoke(
                    null,
                    new object[] { configuration!.Destructure, array });
            }

            return configuration;
        }
    }
}
