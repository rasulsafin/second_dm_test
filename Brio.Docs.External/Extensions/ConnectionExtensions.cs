using System.Collections.Generic;
using Brio.Docs.Integration.Dtos;

namespace Brio.Docs.External.Extensions
{
    public static class ConnectionExtensions
    {
        public static void SetAuthValue(this ConnectionInfoExternalDto info, string key, string value)
        {
            info.AuthFieldValues ??= new Dictionary<string, string>();
            SetDictionaryValue(info.AuthFieldValues, key, value);
        }

        public static void SetAppProperty(this ConnectionInfoExternalDto info, string key, string value)
        {
            info.ConnectionType.AppProperties ??= new Dictionary<string, string>();
            SetDictionaryValue(info.ConnectionType.AppProperties, key, value);
        }

        public static string GetAuthValue(this ConnectionInfoExternalDto info, string key)
        {
            info.AuthFieldValues ??= new Dictionary<string, string>();
            return GetValueOrDefault(info.AuthFieldValues, key);
        }

        public static string GetAppProperty(this ConnectionInfoExternalDto info, string key)
        {
            info.ConnectionType.AppProperties ??= new Dictionary<string, string>();
            return GetValueOrDefault(info.ConnectionType.AppProperties, key);
        }

        private static string GetValueOrDefault(IDictionary<string, string> source, string key)
        {
            if (source != null && source.TryGetValue(key, out var value))
                return value;

            return default;
        }

        private static void SetDictionaryValue(IDictionary<string, string> dictionary, string key, string value)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = value;
                return;
            }

            dictionary.Add(key, value);
        }
    }
}
