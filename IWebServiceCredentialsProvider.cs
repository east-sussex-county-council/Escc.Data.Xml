using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace EsccWebTeam.Data.Xml
{
    /// <summary>
    /// Provides credentials for accessing web services
    /// </summary>
    public interface IWebServiceCredentialsProvider
    {
        /// <summary>
        /// Creates the credentials.
        /// </summary>
        /// <returns></returns>
        ICredentials CreateCredentials();
    }
}
