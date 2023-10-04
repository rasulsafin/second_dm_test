using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Brio.Docs.Client.Exceptions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Brio.Docs.HttpConnection
{
    public class Connection
    {
        private static readonly string CONTENT_TYPE_JSON = "application/json";

        private readonly HttpClient client;
        private readonly ILogger<Connection> logger;

        public Connection(HttpClient client, ILogger<Connection> logger)
        {
            this.client = client;
            this.logger = logger;

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(CONTENT_TYPE_JSON));
        }

        #region Get Methods

        public async Task<T> GetDataAsync<T>(string route, params object[] parameters)
            => await TryGetResponse<T>(route, HttpMethod.Get, routeParams: parameters);

        public async Task<T> GetDataQueryAsync<T>(string route, string query, object[] queryParams = null, params object[] routeParams)
            => await TryGetResponse<T>(route, HttpMethod.Get, query: query, queryParams: queryParams, routeParams: routeParams);

        public async Task<T> GetDataQueryAsync<T>(string route, object queryObject, params object[] routeParams)
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
            };

            var objectString = JsonConvert.SerializeObject(queryObject, settings);
            var objectDictionary = JsonConvert.DeserializeObject<IDictionary<string, string>>(objectString);
            var queryList = objectDictionary.Select(x => x.Key + "=" + x.Value);

            return await TryGetResponse<T>(route, HttpMethod.Get, query: string.Join("&", queryList), routeParams: routeParams);
        }

        #endregion

        #region Delete Methods

        public async Task<T> DeleteDataAsync<T>(string method, params object[] parameters)
            => await TryGetResponse<T>(method, HttpMethod.Delete, routeParams: parameters);

        #endregion

        #region Post Methods

        #region Task

        public async Task PostObjectJsonAsync<TData>(string method, TData data, params object[] parameters)
            => await PostDataAsync(method, new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, CONTENT_TYPE_JSON), parameters);

        public async Task PostDataAsync(string method, Stream stream, params object[] parameters)
            => await PostDataAsync(method, new StreamContent(stream), parameters);

        public async Task PostDataAsync(string method, string data, params object[] parameters)
            => await PostDataAsync(method, new StringContent(data, Encoding.UTF8, CONTENT_TYPE_JSON), parameters);

        public async Task PostDataAsync(string method, byte[] data, params object[] parameters)
            => await PostDataAsync(method, new ByteArrayContent(data), parameters);

        #endregion

        #region Task<T>

        public async Task<TOut> PostObjectJsonAsync<TData, TOut>(string method, TData data, params object[] parameters)
            => await PostDataAsync<TOut>(method, new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, CONTENT_TYPE_JSON), parameters);

        public async Task<TOut> PostObjectJsonQueryAsync<TData, TOut>(string route, string query, object[] queryParams, TData data, params object[] parameters)
            => await TryGetResponse<TOut>(route,
                                        HttpMethod.Post,
                                        new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, CONTENT_TYPE_JSON),
                                        query,
                                        queryParams,
                                        parameters);

        public async Task<T> PostDataAsync<T>(string method, Stream stream, params object[] parameters)
            => await PostDataAsync<T>(method, new StreamContent(stream), parameters);

        public async Task<T> PostDataAsync<T>(string method, string data, params object[] parameters)
            => await PostDataAsync<T>(method, new StringContent(data, Encoding.UTF8, CONTENT_TYPE_JSON), parameters);

        public async Task<T> PostDataAsync<T>(string method, byte[] data, params object[] parameters)
            => await PostDataAsync<T>(method, new ByteArrayContent(data), parameters);

        #endregion

        #endregion

        #region Put Methods

        #region Task

        public async Task PutObjectJsonAsync<TData>(string method, TData data, params object[] parameters)
            => await PutDataAsync(method, new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, CONTENT_TYPE_JSON), parameters);

        public async Task PutDataAsync(string method, Stream stream, params object[] parameters)
            => await PutDataAsync(method, new StreamContent(stream), parameters);

        public async Task PutDataAsync(string method, string data, params object[] parameters)
            => await PutDataAsync(method, new StringContent(data), parameters, Encoding.UTF8, CONTENT_TYPE_JSON);

        public async Task PutDataAsync(string method, byte[] data, params object[] parameters)
            => await PutDataAsync(method, new ByteArrayContent(data), parameters);

        #endregion

        #region Task<T>

        public async Task<TOut> PutObjectJsonAsync<TData, TOut>(string method, TData data, params object[] parameters)
            => await PutDataAsync<TOut>(method, new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, CONTENT_TYPE_JSON), parameters);

        public async Task<T> PutDataAsync<T>(string method, Stream stream, params object[] parameters)
            => await PutDataAsync<T>(method, new StreamContent(stream), parameters);

        public async Task<T> PutDataAsync<T>(string method, string data, params object[] parameters)
            => await PutDataAsync<T>(method, new StringContent(data, Encoding.UTF8, CONTENT_TYPE_JSON), parameters);

        public async Task<T> PutDataAsync<T>(string method, byte[] data, params object[] parameters)
            => await PutDataAsync<T>(method, new ByteArrayContent(data), parameters);

        #endregion

        #endregion

        #region Private Methods

        private async Task PostDataAsync(string method, HttpContent content, params object[] parameters)
            => await TryGetResponse<string>(method, HttpMethod.Post, content: content, routeParams: parameters);

        private async Task<T> PostDataAsync<T>(string method, HttpContent content, params object[] parameters)
            => await TryGetResponse<T>(method, HttpMethod.Post, content: content, routeParams: parameters);

        private async Task PutDataAsync(string method, HttpContent content, params object[] parameters)
            => await TryGetResponse<string>(method, HttpMethod.Put, content: content, routeParams: parameters);

        private async Task<T> PutDataAsync<T>(string method, HttpContent content, params object[] parameters)
            => await TryGetResponse<T>(method, HttpMethod.Put, content: content, routeParams: parameters);

        /// <summary>
        /// Make a request to specified <paramref name="route"/>.
        /// If the request fails on server side <see cref="ConnectionException"/> is thrown.
        /// </summary>
        /// <typeparam name="T">Response object type.</typeparam>
        /// <param name="route">Request URI format string.</param>
        /// <param name="method">Http method type.</param>
        /// <param name="query">Query format string.</param>
        /// <param name="queryParams">Arguments for <paramref name="query"/> format string.</param>
        /// <param name="routeParams">Arguments for <paramref name="route"/> format string.</param>
        /// <returns>Response object.</returns>
        private async Task<T> TryGetResponse<T>(string route,
                                             HttpMethod method,
                                             HttpContent content = default,
                                             string query = "",
                                             object[] queryParams = default,
                                             object[] routeParams = default)
        {
            // Build the uri with params
            var uriBuilder = new UriBuilder(client.BaseAddress)
            {
                Path = string.Format(route, routeParams),
            };

            // Add query to uri
            if (query != string.Empty)
                uriBuilder.Query = queryParams == null ? query : string.Format(query, queryParams);

            // Build HttpRequest
            var request = new HttpRequestMessage(method, uriBuilder.Uri)
            {
                Content = content,
            };

            // Wait for server response
            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                response.Dispose();

                return JsonConvert.DeserializeObject<T>(responseBody);
            }
            else
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                response.Dispose();
                var problemDetails = JsonConvert.DeserializeObject<ProblemDetails>(responseBody);

                logger.LogWarning("Request failed: {@Method} {@Uri} : {@ProblemDetails}", method, uriBuilder.Uri, problemDetails);
                GenerateExceptionByStatusCode(problemDetails);

                // never reached
                return default;
            }
        }

        private void GenerateExceptionByStatusCode(ProblemDetails problem)
        {
            switch (problem.Status)
            {
                case 400:
                    throw new ArgumentValidationException(problem.Title, problem.Detail, problem.Errors);
                case 404:
                    throw new NotFoundException<string>(problem.Title);
                default:
                    throw new DocumentManagementException(problem.Title, problem.Detail);
            }
        }
        #endregion
    }
}
