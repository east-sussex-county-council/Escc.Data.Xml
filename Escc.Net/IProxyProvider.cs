using System.Net;

namespace Escc.Net
{
    public interface IProxyProvider
    {
        /// <summary>
        /// Create a proxy server based on settings in web.config
        /// </summary>
        /// <returns>Configured proxy object, or <c>null</c> if settings not found</returns>
        IWebProxy CreateProxy();
    }
}