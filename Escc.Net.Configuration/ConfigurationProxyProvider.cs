using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.Net;

namespace Escc.Net.Configuration
{
    /// <summary>
    /// Read proxy server settings from <c>web.config</c> or <c>app.config</c>.
    /// </summary>
    public class ConfigurationProxyProvider : IProxyProvider
    {
        /// <summary>
        /// Create a proxy server based on settings in <c>web.config</c> or <c>app.config</c>.
        /// </summary>
        /// <returns>Configured proxy object, or <c>null</c> if settings not found</returns>
        public IWebProxy CreateProxy()
        {
            // if no proxy configured, don't return a proxy
            var config = GetConfigurationSettings();
            if (config == null || String.IsNullOrEmpty(config["Server"])) return null;
            
            var proxy = new WebProxy()
            {
                Address = GetProxyUrl(),
                Credentials = CreateCredentials()
            };
            return proxy;
        }

        /// <summary>
        /// Gets the configuration settings, supporting both old and up-to-date section names.
        /// </summary>
        /// <returns></returns>
        private static NameValueCollection GetConfigurationSettings()
        {
            var config = ConfigurationManager.GetSection("Escc.Net/Proxy") as NameValueCollection;
            if (config == null)
            {
                config = ConfigurationManager.GetSection("EsccWebTeam.Data.Xml/Proxy") as NameValueCollection;
            }
            return config;
        }

        /// <summary>
        /// Creates the credentials used to authenticate with the proxy server.
        /// </summary>
        /// <returns></returns>
        private static ICredentials CreateCredentials()
        {
            // If no account in web.config, just use current credentials
            var config = GetConfigurationSettings();
            if (config == null || String.IsNullOrEmpty(config["User"])) return CredentialCache.DefaultCredentials;

            // Otherwise get from config
            var user = config["User"];
            var password = String.IsNullOrEmpty(config["Password"]) ? String.Empty : config["Password"];

            // Separate "domain" setting is optional, as NetworkCredential will parse domain\user format
            if (String.IsNullOrEmpty(config["Domain"]))
            {
                return new NetworkCredential(user, password);

            }
            else
            {
                return new NetworkCredential(user, password, config["Domain"]);
            }
        }

        /// <summary>
        /// Returns the URL of a proxy server.
        /// </summary>
        private static Uri GetProxyUrl()
        {
            // Load settings from web.config which allow requests to go out through a proxy server
            var config = GetConfigurationSettings();
            if (config == null || String.IsNullOrEmpty(config["Server"])) return null;

            if (config["Server"].Contains("://"))
            {
                return new Uri(config["Server"]);
            }
            else
            {
                // If only a hostname or IP specified, assume HTTP for backwards compatibility
                return new Uri("http://" + config["Server"]);
            }
        }
    }
}
