using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Escc.Net
{
    /// <summary>
    /// Configuration settings for the <c>Escc.Net</c> namespace
    /// </summary>
    public class ConfigurationSettings
    {
        /// <summary>
        /// Gets or sets settings for connecting to a proxy server.
        /// </summary>
        public ProxySettings Proxy { get; set; }

        /// <summary>
        /// Gets or sets settings to use when calling Web APIs
        /// </summary>
        public WebApiSettings WebApi { get; set; }
    }

    /// <summary>
    /// Settings to use when called Web APIs
    /// </summary>
    public class WebApiSettings
    {
        /// <summary>
        /// Gets or sets the username in the format domain\user.
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        public string Password { get; set; }
    }

    /// <summary>
    /// Settings for connecting to a proxy server
    /// </summary>
    public class ProxySettings
    {
        /// <summary>
        /// Gets or sets the URL of the proxy server.
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// Gets or sets the username in the format domain\user.
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        public string Password { get; set; }
    }
}
