using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Brio.Docs.Integration.Dtos;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Util.Store;
using Newtonsoft.Json;

namespace Brio.Docs.Connections.GoogleDrive
{

    public class DataStore : IDataStore
    {
        private ConnectionInfoExternalDto info;

        public DataStore(ConnectionInfoExternalDto info)
        {
            this.info = info;
            CheckAndCreateDictionary();
        }

        public Task ClearAsync()
        {
            info.AuthFieldValues.Clear();
            return Task.CompletedTask;
        }

        private void CheckAndCreateDictionary()
        {
            if (info.AuthFieldValues == null)
                info.AuthFieldValues = new Dictionary<string, string>();
        }

        public Task DeleteAsync<T>(string key)
        {
            info.AuthFieldValues.Remove(key);
            return Task.CompletedTask;
        }

        public Task<T> GetAsync<T>(string key)
        {
            if (info.AuthFieldValues.TryGetValue(key, out string value))
            {
                var type = typeof(T);

                if (type == typeof(string) && value is T resultStr)
                {
                    return Task.FromResult(resultStr);
                }
                else if (type == typeof(int))
                {
                    if (int.TryParse(value, out int tempInt) && tempInt is T resultInt)
                        return Task.FromResult(resultInt);
                }
                else if (type == typeof(TokenResponse))
                {
                    try
                    {
                        var token = JsonConvert.DeserializeObject<TokenResponse>(value);
                        if (token is T resultToken)
                            return Task.FromResult(resultToken);
                    }
                    catch { }
                }
                
                else
                {
                    throw new NotImplementedException($"Тип не реализован {type.Name}");
                }
            }

            return Task.FromResult(default(T));
        }

        public Task StoreAsync<T>(string key, T obj)
        {
            string value = JsonConvert.SerializeObject(obj);

            if (!info.AuthFieldValues.ContainsKey(key))
                info.AuthFieldValues.Add(key, value);
            else
                info.AuthFieldValues[key] = value;
            return Task.CompletedTask;
        }
    }
}
