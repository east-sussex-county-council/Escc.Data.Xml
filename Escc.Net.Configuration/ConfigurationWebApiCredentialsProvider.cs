using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Net;

namespace Escc.Net.Configuration
{
    /// <summary>
    /// Reads credentials for accessing web services from web.config or app.config
    /// </summary>
    public class ConfigurationWebApiCredentialsProvider : IWebApiCredentialsProvider
    {
        /// <summary>
        /// Creates the credentials.
        /// </summary>
        /// <returns></returns>
        public ICredentials CreateCredentials()
        {
            // Start by looking for configuration settings consistent with other classes in this project
            var config = ConfigurationManager.GetSection("Escc.Net/WebApi") as NameValueCollection;
            if (config != null && !String.IsNullOrEmpty(config["User"]))
            {
                var user = config["User"];
                var password = String.IsNullOrEmpty(config["Password"]) ? String.Empty : config["Password"];
                return new NetworkCredential(user, password);
            }
            
            // Fall back to using the current credentials
            return CredentialCache.DefaultCredentials;
        }
    }
}
