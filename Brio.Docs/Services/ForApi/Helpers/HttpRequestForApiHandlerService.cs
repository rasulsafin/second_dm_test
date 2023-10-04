using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Brio.Docs.Client.Services.ForApi.Helpers;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Brio.Docs.Services.ForApi.Helpers
{
    public class HttpRequestForApiHandlerService : IHttpRequestForApiHandlerService, IDisposable
    {
        private readonly HttpClient httpClient;

        public HttpRequestForApiHandlerService(IConfiguration configuration)
        {
            var apiConfig = configuration.GetSection("DMApi");

            this.httpClient = new HttpClient
            {
                BaseAddress = new Uri(apiConfig["BaseAddress"]),
            };
        }

        public async Task<HttpResponseMessage> SendGetRequest(string uri)
        {
            var response = await httpClient.GetAsync(uri);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"HTTP GET Request failed with status code: {response.StatusCode}");
            }

            return response;
        }

        public async Task<HttpResponseMessage> SendPostRequest(string uri, object data)
        {
            var jsonData = JsonConvert.SerializeObject(data);
            var httpContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(uri, httpContent);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"HTTP POST Request failed with status code: {response.StatusCode}");
            }

            return response;
        }

        public async Task<HttpResponseMessage> SendDeleteRequest(string uri)
        {
            var response = await httpClient.DeleteAsync(uri);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"HTTP GET Request failed with status code: {response.StatusCode}");
            }

            return response;
        }

        public async Task<HttpResponseMessage> SendPutRequest(string uri, object data)
        {
            var jsonData = JsonConvert.SerializeObject(data);
            var httpContent = new StringContent(jsonData, Encoding.UTF8, "application/json");

            var response = await httpClient.PutAsync(uri, httpContent);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"HTTP PUT Request failed with status code: {response.StatusCode}");
            }

            return response;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                httpClient.Dispose();
            }
        }
    }
}
