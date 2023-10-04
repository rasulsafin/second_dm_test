using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Brio.Docs.Client.Services.ForApi.Helpers
{
    /// <summary>
    /// Helper service to make http requests to the API.
    /// </summary>
    /// <param name="uri"></param>
    /// <returns></returns>
    public interface IHttpRequestForApiHandlerService
    {
        /// <summary>
        /// Helper method to make GET method to the API.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        Task<HttpResponseMessage> SendGetRequest(string uri);

        /// <summary>
        /// Helper method to make POST method to the API.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        Task<HttpResponseMessage> SendPostRequest(string uri, object data);

        /// <summary>
        /// Helper method to make DELETE method to the API.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        Task<HttpResponseMessage> SendDeleteRequest(string uri);

        /// <summary>
        /// Helper method to make PUT method to the API.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        Task<HttpResponseMessage> SendPutRequest(string uri, object data);
    }
}
