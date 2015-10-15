using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Policy;
using System.Text;
using Newtonsoft.Json;

namespace Escc.Net
{
    /// <summary>
    /// Make JSON requests to .NET Web APIs
    /// </summary>
    public class WebApiClient : IWebApiClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebApiClient"/> class.
        /// </summary>
        public WebApiClient()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebApiClient"/> class.
        /// </summary>
        /// <param name="credentialsProvider">A strategy for getting credentials for the web request.</param>
        public WebApiClient(IWebApiCredentialsProvider credentialsProvider)
        {
            CredentialsProvider = credentialsProvider;
        }

        /// <summary>
        /// Gets or sets the credentials provider.
        /// </summary>
        /// <value>
        /// The credentials provider.
        /// </value>
        public IWebApiCredentialsProvider CredentialsProvider { get; set; }

        /// <summary>
        /// Gets data from the specified URL and returns an object of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        public T Get<T>(Uri url)
        {
            var request = PrepareJsonRequest(url, "GET", CredentialsProvider);

            return ReturnObjectFromResponse<T>(request);
        }

        /// <summary>
        /// Post data to the specified URL without returning an object.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="data">The data.</param>
        public void Post(Uri url, object data)
        {
            var request = PrepareJsonRequestWithBody(url, "POST", data);
            using (request.GetResponse()) { }
        }

        /// <summary>
        /// Posts data to the specified URL and returns an object of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public T Post<T>(Uri url, object data)
        {
            var request = PrepareJsonRequestWithBody(url, "POST", data);

            return ReturnObjectFromResponse<T>(request);
        }

        /// <summary>
        /// Puts data to the specified URL without returning an object.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="data">The data.</param>
        public void Put(Uri url, object data)
        {
            var request = PrepareJsonRequestWithBody(url, "PUT", data);

            using (request.GetResponse()) { }
        }

        /// <summary>
        /// Puts data to the specified URL and returns an object of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public T Put<T>(Uri url, object data)
        {
            var request = PrepareJsonRequestWithBody(url, "PUT", data);

            return ReturnObjectFromResponse<T>(request);
        }

        private WebRequest PrepareJsonRequestWithBody(Uri url, string method, object data)
        {
            var request = PrepareJsonRequest(url, method, CredentialsProvider);

            string postData = JsonConvert.SerializeObject(data);
            var encoding = new ASCIIEncoding();
            byte[] postDataAsBytes = encoding.GetBytes(postData);

            request.ContentLength = postDataAsBytes.Length;
            request.ContentType = "application/json";

            using (var requestStream = request.GetRequestStream())
            {
                requestStream.Write(postDataAsBytes, 0, postDataAsBytes.Length);
            }

            return request;
        }

        private static WebRequest PrepareJsonRequest(Uri url, string method, IWebApiCredentialsProvider credentialsProvider)
        {
            var request = WebRequest.Create(url);
            if (credentialsProvider != null)
            {
                request.Credentials = credentialsProvider.CreateCredentials();
            }
            request.Method = method;
            return request;
        }

        private static T ReturnObjectFromResponse<T>(WebRequest request)
        {
            using (var response = request.GetResponse())
            {
                using (var responseReader = new StreamReader(response.GetResponseStream()))
                {
                    var responseJson = responseReader.ReadToEnd();
                    return JsonConvert.DeserializeObject<T>(responseJson);
                }
            }
        }
    }
}
