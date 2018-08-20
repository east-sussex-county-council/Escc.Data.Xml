using System;
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
    }
}
