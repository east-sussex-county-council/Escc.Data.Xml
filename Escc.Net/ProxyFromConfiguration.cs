using Microsoft.Extensions.Options;
using System;
using System.Net;

namespace Escc.Net
{
    /// <summary>
    /// Read proxy server settings from .NET Standard configuration.
    /// </summary>
    public class ProxyFromConfiguration : IProxyProvider
    {
        private readonly ProxySettings _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyFromConfiguration" /> class.
        /// </summary>
        /// <param name="configuration">The configuration settings.</param>
        public ProxyFromConfiguration(IOptions<ConfigurationSettings> configuration)
        {
            _configuration = configuration.Value.Proxy;
        }

        /// <summary>
        /// Create a proxy server based on settings in <c>web.config</c> or <c>app.config</c>.
        /// </summary>
        /// <returns>Configured proxy object, or <c>null</c> if settings not found</returns>
        public IWebProxy CreateProxy()
        {
            // if no proxy configured, don't return a proxy
            if (_configuration == null || String.IsNullOrEmpty(_configuration.Server)) return null;
            
            var proxy = new WebProxy()
            {
                Address = GetProxyUrl(),
                Credentials = CreateCredentials()
            };
            return proxy;
        }

        /// <summary>
        /// Creates the credentials used to authenticate with the proxy server.
        /// </summary>
        /// <returns></returns>
        private ICredentials CreateCredentials()
        {
            // If no account in config, just use current credentials
            if (_configuration == null || String.IsNullOrEmpty(_configuration.User)) return CredentialCache.DefaultCredentials;

            // Otherwise get from config
            var user = _configuration.User;
            var password = String.IsNullOrEmpty(_configuration.Password) ? String.Empty : _configuration.Password;

            return new NetworkCredential(user, password);
        }

        /// <summary>
        /// Returns the URL of a proxy server.
        /// </summary>
        private Uri GetProxyUrl()
        {
            // Load settings from config which allow requests to go out through a proxy server
            if (_configuration == null || String.IsNullOrEmpty(_configuration.Server)) return null;

            if (_configuration.Server.Contains("://"))
            {
                return new Uri(_configuration.Server);
            }
            else
            {
                // If only a hostname or IP specified, assume HTTP for backwards compatibility
                return new Uri("http://" + _configuration.Server);
            }
        }
    }
}
