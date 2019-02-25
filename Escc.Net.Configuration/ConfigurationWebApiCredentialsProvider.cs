using System;
using System.Collections.Generic;
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
        private NameValueCollection _configurationData;

        /// <summary>
        /// Creates a new instance of <see cref="ConfigurationWebApiCredentialsProvider"/>
        /// </summary>
        /// <param name="configurationData">Configurationd data. If <c>null</c> this will be read from web.config or app.config</param>
        public ConfigurationWebApiCredentialsProvider(NameValueCollection configurationData=null)
        {
            _configurationData = configurationData ?? ConfigurationManager.GetSection("Escc.Net/WebApi") as NameValueCollection;
        }

        /// <summary>
        /// Creates the credentials.
        /// </summary>
        /// <returns></returns>
        public ICredentials CreateCredentials()
        {
            // Start by looking for configuration settings consistent with other classes in this project
            if (_configurationData != null && !String.IsNullOrEmpty(_configurationData["User"]))
            {
                var user = _configurationData["User"];
                var password = String.IsNullOrEmpty(_configurationData["Password"]) ? String.Empty : _configurationData["Password"];
                return new NetworkCredential(user, password);
            }
            
            // Fall back to using the current credentials
            return CredentialCache.DefaultCredentials;
        }


        /// <summary>
        /// Compares this instance with another instance, and determines that they are equal if the configured values they read are the same
        /// </summary>
        /// <param name="obj">Another instance of <see cref="ConfigurationWebApiCredentialsProvider"/></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var other = obj as ConfigurationWebApiCredentialsProvider;
            if (other != null)
            {
                var thisConfig = (_configurationData != null) ? _configurationData["User"] + _configurationData["Password"] : string.Empty;
                var otherConfig = (other._configurationData != null) ? other._configurationData["User"] + other._configurationData["Password"] : string.Empty;
                return thisConfig == otherConfig;
            }
            else { return base.Equals(obj); }
        }

        /// <summary>
        /// Gets a hash for comparison based on the hashes of the configured username and password
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return 809550988 + EqualityComparer<string>.Default.GetHashCode(_configurationData?["User"]) + EqualityComparer<string>.Default.GetHashCode(_configurationData?["Password"]);
        }

        /// <summary>
        /// Compares this instance with another instance, and determines that they are equal if the configured values they read are the same
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        public static bool operator ==(ConfigurationWebApiCredentialsProvider obj1, ConfigurationWebApiCredentialsProvider obj2)
        {
            if (ReferenceEquals(obj1, obj2))
            {
                return true;
            }

            if (ReferenceEquals(obj1, null))
            {
                return false;
            }

            if (ReferenceEquals(obj2, null))
            {
                return false;
            }

            return obj1.Equals(obj2);
        }

        /// <summary>
        /// Compares this instance with another instance, and determines that they are not equal if the configured values they read are different
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        public static bool operator !=(ConfigurationWebApiCredentialsProvider obj1, ConfigurationWebApiCredentialsProvider obj2)
        {
            return !(obj1 == obj2);
        }
    }
}
