using System;
using System.Configuration;
using System.Net;

namespace EsccWebTeam.Data.Xml
{
    /// <summary>
    /// Reads credentials for accessing web services from web.config or app.config
    /// </summary>
    public class WebServiceConfigurationCredentialsProvider : IWebServiceCredentialsProvider
    {
        /// <summary>
        /// Creates the credentials.
        /// </summary>
        /// <returns></returns>
        public ICredentials CreateCredentials()
        {
            if (!String.IsNullOrEmpty(ConfigurationManager.AppSettings["ConnectToCouncilWebServicesAccount"]) &&
                !String.IsNullOrEmpty(ConfigurationManager.AppSettings["ConnectToCouncilWebServicesPassword"]))
            {
                return new NetworkCredential(ConfigurationManager.AppSettings["ConnectToCouncilWebServicesAccount"], ConfigurationManager.AppSettings["ConnectToCouncilWebServicesPassword"]);
            }
            else
            {
                return CredentialCache.DefaultCredentials;
            }
        }
    }
}
