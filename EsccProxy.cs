#region Using Directives
using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.Net;
#endregion

namespace EsccWebTeam.Data.Xml
{
    /// <summary>
    /// A class designed to assist making web requests with the correct credentials via the proxy server.
    /// </summary>
    public class EsccProxy : IWebProxy
    {
        /// <summary>
        /// The credentials to submit to the proxy server for authentication.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// An <see cref="T:System.Net.ICredentials"/> instance that contains the credentials that are needed to authenticate a request to the proxy server.
        /// </returns>
        public ICredentials Credentials
        {
            get
            {
                string user = null;
                string password = null;
                string domain = null;

                // Load settings from web.config which allow requests to go out through the ESCC proxy server
                NameValueCollection config = ConfigurationManager.GetSection("EsccWebTeam.Data.Xml/Proxy") as NameValueCollection;

                if (config["User"] != null)
                {
                    user = config["User"];
                    password = (config["Password"] != null) ? config["Password"] : String.Empty;
                    domain = (config["Domain"] != null) ? config["Domain"] : String.Empty;

                    return new NetworkCredential(user, password, domain);
                }
                else
                {
                    // If no account in web.config, just use current credentials
                    return System.Net.CredentialCache.DefaultCredentials;
                }
            }
            set { }
        }

        /// <summary>
        /// Returns the URI of a proxy.
        /// </summary>
        /// <param name="destination">A <see cref="T:System.Uri"/> that specifies the requested Internet resource.</param>
        /// <returns>
        /// A <see cref="T:System.Uri"/> instance that contains the URI of the proxy used to contact <paramref name="destination"/>.
        /// </returns>
        public Uri GetProxy(Uri destination)
        {
            // Load settings from web.config which allow requests to go out through the ESCC proxy server
            NameValueCollection config = ConfigurationManager.GetSection("EsccWebTeam.Data.Xml/Proxy") as NameValueCollection;

            string url = string.Format(CultureInfo.InvariantCulture, "http://{0}", config["Server"]);

            return new Uri(url);
        }

        /// <summary>
        /// Indicates that the proxy should not be used for the specified host.
        /// </summary>
        /// <param name="host">The <see cref="T:System.Uri"/> of the host to check for proxy use.</param>
        /// <returns>
        /// true if the proxy server should not be used for <paramref name="host"/>; otherwise, false.
        /// </returns>
        public bool IsBypassed(Uri host)
        {
            return false;
        }
    }
}
