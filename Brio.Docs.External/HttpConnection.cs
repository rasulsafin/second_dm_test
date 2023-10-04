using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Brio.Docs.External
{
    public class HttpConnection : IDisposable
    {
        protected readonly HttpClient client;
        protected readonly JsonSerializerSettings jsonSerializerSettings =
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };

        private static readonly int MAX_ATTEMPTS = 5;
        private static readonly int ATTEMPT_DELAY = 2000;

        public HttpConnection()
            => client = new HttpClient { Timeout = TimeSpan.FromSeconds(Timeout) };

        protected double Timeout { get; set; } = 10;

        public void Dispose()
        {
            client.Dispose();
            GC.SuppressFinalize(this);
        }

        public async Task<Stream> GetResponseStreamAuthorizedAsync(
                HttpMethod methodType,
                string command,
                (string scheme, string token) authData = default,
                params object[] arguments)
        {
            var response = await SendRequestAsync(
                () => CreateRequest(methodType, command, arguments, authData),
                completionOption: HttpCompletionOption.ResponseHeadersRead);
            return await response.Content.ReadAsStreamAsync();
        }

        public virtual HttpRequestMessage CreateRequest(
                HttpMethod methodType,
                string uri,
                object[] arguments = null,
                (string scheme, string token) authData = default)
        {
            var argumentsArray = arguments ?? Array.Empty<object>();
            var formattedUri = string.Format(uri, argumentsArray);
            var request = new HttpRequestMessage(methodType, formattedUri);
            if (authData != default)
                request.Headers.Authorization = new AuthenticationHeaderValue(authData.scheme, authData.token);

            return request;
        }

        public async Task<JToken> GetResponseAsync(
            Func<HttpRequestMessage> createRequest,
            (string scheme, string token) authData = default)
        {
            using var response = await SendRequestAsync(createRequest, authData);
            var content = await response.Content.ReadAsStringAsync();
            var jToken = JToken.Parse(content);
            return jToken;
        }

        public async Task<Uri> GetUriAsync(
            Func<HttpRequestMessage> createRequest,
            (string scheme, string token) authData = default)
        {
            using var response = await SendRequestAsync(createRequest, authData);
            var uri = response.RequestMessage.RequestUri;
            return uri;
        }

        public async Task<HttpResponseMessage> SendRequestAsync(
            Func<HttpRequestMessage> createRequest,
            (string scheme, string token) authData = default,
            HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
        {
            HttpStatusCode code = default;
            HttpContent content = default;
            Exception exception = default;

            for (int i = 0; i < MAX_ATTEMPTS; i++)
            {
                using var request = createRequest();

                try
                {
                    if (authData != default)
                        request.Headers.Authorization = new AuthenticationHeaderValue(authData.scheme, authData.token);
                    var response = await client.SendAsync(request, completionOption);

                    if (response.IsSuccessStatusCode)
                        return response;

                    code = response.StatusCode;
                    content = response.Content;

                    if (code is > HttpStatusCode.BadRequest and < HttpStatusCode.UnavailableForLegalReasons and not
                        HttpStatusCode.TooManyRequests)
                        response.EnsureSuccessStatusCode();
                }
                catch (TaskCanceledException e)
                {
                    exception = e;
                }

                await Task.Delay(ATTEMPT_DELAY);
            }

            if (exception != null)
                throw exception;

            throw new HttpRequestException(
                $"{code} Response status code does not indicate success",
                new Exception(content == null ? null : await content.ReadAsStringAsync()),
                code);
        }
    }
}
