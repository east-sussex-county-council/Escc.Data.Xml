using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Policy;
using System.Text;
using Newtonsoft.Json;

namespace EsccWebTeam.Data.Xml
{
    /// <summary>
    /// Make JSON requests to .NET Web APIs
    /// </summary>
    public class WebApiRequest
    {
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
            var request = WebRequest.Create(url);
            request.Credentials = new WebServiceConfigurationCredentialsProvider().CreateCredentials();
            request.Method = method;

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

        private T ReturnObjectFromResponse<T>(WebRequest request)
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
