using System;

namespace Escc.Net
{
    /// <summary>
    /// A helper for making requests to a .NET Web API
    /// </summary>
    public interface IWebApiClient
    {
        /// <summary>
        /// Gets data from the specified URL and returns an object of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Get")]
        T Get<T>(Uri url);

        /// <summary>
        /// Post data to the specified URL without returning an object.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="data">The data.</param>
        void Post(Uri url, object data);

        /// <summary>
        /// Posts data to the specified URL and returns an object of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        T Post<T>(Uri url, object data);

        /// <summary>
        /// Puts data to the specified URL without returning an object.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="data">The data.</param>
        void Put(Uri url, object data);

        /// <summary>
        /// Puts data to the specified URL and returns an object of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        T Put<T>(Uri url, object data);
    }
}