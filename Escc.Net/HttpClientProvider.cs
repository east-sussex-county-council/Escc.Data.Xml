using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace Escc.Net
{
    /// <summary>
    /// Gets a singleton instance of HttpClient which can be shared by all code within an application
    /// </summary>
    public class HttpClientProvider : IHttpClientProvider
    {
        private static Dictionary<IWebApiCredentialsProvider, HttpClient> _httpClients = new Dictionary<IWebApiCredentialsProvider, HttpClient>();
        private readonly IProxyProvider _proxyProvider;
        private readonly IWebApiCredentialsProvider _credentialsProvider;

        /// <summary>
        /// Creates a new instance of <see cref="HttpClientProvider"/>
        /// </summary>
        /// <param name="proxyProvider"></param>
        /// <param name="credentialsProvider"></param>
        public HttpClientProvider(IProxyProvider proxyProvider=null, IWebApiCredentialsProvider credentialsProvider=null)
        {
            _proxyProvider = proxyProvider;
            _credentialsProvider = credentialsProvider ?? new DefaultCredentialsProvider();
        }

        /// <summary>
        /// Gets a singleton instance of HttpClient 
        /// </summary>
        /// <returns></returns>
        public HttpClient GetHttpClient()
        {
            // It is expected that the same proxy will be relevant for all requests, but credentials may differ so
            // maintain a pool of HttpClients for each unique set of credentials.
            if (!_httpClients.ContainsKey(_credentialsProvider))
            {
                _httpClients.Add(_credentialsProvider,
                    new HttpClient(new HttpClientHandler
                    {
                        Proxy = _proxyProvider?.CreateProxy(),
                        Credentials = _credentialsProvider?.CreateCredentials() ?? CredentialCache.DefaultCredentials
                    }));
            }
            return _httpClients[_credentialsProvider];
        }

        /// <summary>
        /// Credentials provider to be used when no provider is passed to the constructor. This means the provider is never null, so can be used to index a Dictionary.
        /// </summary>
        private class DefaultCredentialsProvider : IWebApiCredentialsProvider
        {
            public ICredentials CreateCredentials()
            {
                return CredentialCache.DefaultCredentials;
            }
        }
    }
}
