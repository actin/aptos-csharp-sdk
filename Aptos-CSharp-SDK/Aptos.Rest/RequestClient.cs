using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;

namespace Aptos.Rest
{

    /// <summary>
    /// UnityWebRequest wrapper client
    /// </summary>
    public class RequestClient
    {

        public static readonly  string     X_APTOS_HEADER = "x-aptos-client";
        private static readonly HttpClient httpClient     = new();

        /// <summary>
        /// Get the default Aptos header value
        /// </summary>
        /// <returns>String with the default Aptos header value</returns>
        public static string GetAptosHeaderValue()
        {
            return $"aptos-csharp-sdk/1";
        }

        /// <summary>
        /// Get the HttpRequestMessage object for the given path, default to GET method
        /// </summary>
        /// <param name="uri">endpoint uri</param>
        /// <param name="method">HTTP method</param>
        /// <returns>HttpRequestMessage object</returns>
        public static HttpRequestMessage SubmitRequest(Uri uri, HttpMethod method = null)
        {
            method ??= HttpMethod.Get;
            var request = new HttpRequestMessage(method, uri);

            // Set the default Aptos header
            request.Headers.Add(X_APTOS_HEADER, GetAptosHeaderValue());
            return request;
        }

        public static async Task<HttpResponseMessage> GetAsync(Uri uri)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, uri);

            // Set the default Aptos header
            request.Headers.Add(X_APTOS_HEADER, GetAptosHeaderValue());

            return await SendAsync(request);
        }

        public static async Task<HttpResponseMessage> PostAsync(Uri uri, string data)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = new StringContent(data, Encoding.UTF8, "application/json")
            };

            // Set the default Aptos header
            request.Headers.Add(X_APTOS_HEADER, GetAptosHeaderValue());

            return await SendAsync(request);
        }

        private static async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            var response = await httpClient.SendAsync(request);

            return response;
        }

    }
}