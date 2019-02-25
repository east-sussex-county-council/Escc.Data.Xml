using System.Net.Http;

namespace Escc.Net
{
    /// <summary>
    /// Gets a singleton instance of HttpClient which can be shared by all code within an application
    /// </summary>
    public interface IHttpClientProvider
    {
        /// <summary>
        /// Gets a singleton instance of HttpClient 
        /// </summary>
        /// <returns></returns>
        HttpClient GetHttpClient();
    }
}