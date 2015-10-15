using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Escc.Net
{
    /// <summary>
    /// Provides credentials for accessing web services
    /// </summary>
    public interface IWebApiCredentialsProvider
    {
        /// <summary>
        /// Creates the credentials.
        /// </summary>
        /// <returns></returns>
        ICredentials CreateCredentials();
    }
}
