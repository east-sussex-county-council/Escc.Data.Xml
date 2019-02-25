using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.Extensions.Options;

namespace Escc.Net
{
    /// <summary>
    /// Reads credentials for accessing web services from .NET Standard configuration 
    /// </summary>
    public class WebApiCredentialsFromConfiguration : IWebApiCredentialsProvider
    {
        private readonly WebApiSettings _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebApiCredentialsFromConfiguration" /> class.
        /// </summary>
        /// <param name="configuration">The configuration settings.</param>
        public WebApiCredentialsFromConfiguration(IOptions<ConfigurationSettings> configuration)
        {
            _configuration = configuration.Value.WebApi;
        }

        /// <summary>
        /// Creates the credentials.
        /// </summary>
        /// <returns></returns>
        public ICredentials CreateCredentials()
        {
            // Start by looking for configuration settings consistent with other classes in this project
            if (_configuration != null && !String.IsNullOrEmpty(_configuration.User))
            {
                var user = _configuration.User;
                var password = String.IsNullOrEmpty(_configuration.Password) ? String.Empty : _configuration.Password;
                return new NetworkCredential(user, password);
            }
            
            // Fall back to using the current credentials
            return CredentialCache.DefaultCredentials;
        }

        /// <summary>
        /// Compares this instance with another instance, and determines that they are equal if the configured values they read are the same
        /// </summary>
        /// <param name="obj">Another instance of <see cref="WebApiCredentialsFromConfiguration"/></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var other = obj as WebApiCredentialsFromConfiguration;
            if (other != null)
            {
                var thisConfig = (_configuration != null) ? _configuration.User + _configuration.Password : string.Empty;
                var otherConfig = (other._configuration != null) ? other._configuration.User + other._configuration.Password : string.Empty;
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
            return -1607759644 + EqualityComparer<string>.Default.GetHashCode(_configuration?.User) + EqualityComparer<string>.Default.GetHashCode(_configuration?.Password);
        }

        /// <summary>
        /// Compares this instance with another instance, and determines that they are equal if the configured values they read are the same
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        public static bool operator == (WebApiCredentialsFromConfiguration obj1, WebApiCredentialsFromConfiguration obj2)
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
        public static bool operator !=(WebApiCredentialsFromConfiguration obj1, WebApiCredentialsFromConfiguration obj2)
        {
            return !(obj1 == obj2);
        }
    }
}
