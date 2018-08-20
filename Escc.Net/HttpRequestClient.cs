using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace Escc.Net
{
    /// <summary>
    /// Helper class for requesting online resources, with specialised methods for XML data
    /// </summary>
    public class HttpRequestClient : IHttpRequestClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestClient"/> class.
        /// </summary>
        public HttpRequestClient()
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestClient"/> class.
        /// </summary>
        /// <param name="proxyProvider">The provider for configuring a proxy server.</param>
        public HttpRequestClient(IProxyProvider proxyProvider)
        {
            ProxyProvider = proxyProvider;
        }

        /// <summary>
        /// Gets or sets the provider for configuring a proxy server.
        /// </summary>
        /// <value>
        /// The proxy provider.
        /// </value>
        public IProxyProvider ProxyProvider { get; set; }

        /// <summary>
        /// Creates a new <see cref="HttpWebRequest"/> for the specified URI, with proxy access configured.
        /// </summary>
        /// <param name="requestUri">The request URI.</param>
        /// <returns></returns>
        public HttpWebRequest CreateRequest(Uri requestUri)
        {
            var webRequest = (HttpWebRequest)WebRequest.Create(requestUri);
            webRequest.UserAgent = "Escc.Net.HttpRequestClient"; // Some apps require a user-agent to be present
            if (ProxyProvider != null)
            {
                IWebProxy proxy = ProxyProvider.CreateProxy();
                if (proxy != null)
                {
                    webRequest.Proxy = proxy;
                    webRequest.Credentials = proxy.Credentials;
                }
                else
                {
                    webRequest.Credentials = CredentialCache.DefaultCredentials;
                }
            }
            else
            {
                webRequest.Credentials = CredentialCache.DefaultCredentials;
            }
            return webRequest;
        }

        /// <summary>
        /// Requests an URL and loads the response into a string
        /// </summary>
        /// <param name="requestUrl">The URL.</param>
        /// <exception cref="WebException">Thrown if response has an HTTP status other than 200 OK</exception>
        /// <returns></returns>
        public string RequestString(Uri requestUrl)
        {
            string responseString = String.Empty;
            var webRequest = CreateRequest(requestUrl);
            using (var webResponse = webRequest.GetResponse() as HttpWebResponse)
            {
                if (webResponse.StatusCode == HttpStatusCode.OK)
                {
                    using (var reader = new StreamReader(webResponse.GetResponseStream()))
                    {
                        responseString = reader.ReadToEnd();
                    }
                }
                else
                {
                    throw new WebException(String.Format(CultureInfo.InvariantCulture, "Request received an HTTP response of {0} {1}", webResponse.StatusCode, webResponse.StatusDescription));
                }
            }
            return responseString;
        }

        /// <summary>
        /// Requests XML data from a URL, transforms the response and returns the result as a string
        /// </summary>
        /// <param name="requestUrl">The request URL.</param>
        /// <param name="xsltUrl">The XSLT URL.</param>
        /// <param name="xsltArguments">The XSLT arguments.</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public string RequestString(Uri requestUrl, Uri xsltUrl, XsltArgumentList xsltArguments)
        {
            if (xsltUrl == null) throw new ArgumentNullException("xsltUrl");

            string transformResult = String.Empty;
            MemoryStream ms = null;
            try
            {
                ms = new MemoryStream();
                var transform = new XslCompiledTransform();
                transform.Load(xsltUrl.ToString());
                transform.Transform(RequestXPath(requestUrl), xsltArguments, ms);
                ms.Position = 0;
                using (var reader = new StreamReader(ms))
                {
                    transformResult = reader.ReadToEnd();
                }
            }
            finally
            {
                if (ms != null)
                {
                    ms.Dispose();
                }
            }
            return transformResult;
        }

        /// <summary>
        /// Requests XML data from a URL and loads the response into an XPathDocument
        /// </summary>
        /// <param name="requestUrl">The URL.</param>
        /// <exception cref="WebException">Thrown if XML response has an HTTP status other than 200 OK</exception>
        /// <returns></returns>
        public IXPathNavigable RequestXPath(Uri requestUrl)
        {
            if (requestUrl == null) throw new ArgumentNullException("requestUrl");
            return RequestXPath(CreateRequest(requestUrl));
        }

        /// <summary>
        /// Requests XML data from a URL and loads the response into an XPathDocument
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        /// <exception cref="WebException">Thrown if XML response has an HTTP status other than 200 OK</exception>
        public IXPathNavigable RequestXPath(WebRequest request)
        {
            return RequestXPath(request, 0);
        }

        /// <summary>
        /// Requests XML data from a URL and loads the response into an XPathDocument
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="retries">Amount of times to retry a query. Maximum is 2.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">request</exception>
        /// <exception cref="System.Net.WebException"></exception>
        /// <exception cref="WebException">Thrown if XML response has an HTTP status other than 200 OK</exception>
        public IXPathNavigable RequestXPath(WebRequest request, int retries)
        {
            if (request == null) throw new ArgumentNullException("request");

            // Limit retries to avoid getting into a loop and causing accidental denial of service 
            if (retries < 0) retries = 0;
            if (retries > 2) retries = 2;

            try
            {
                using (var xmlResponse = request.GetResponse() as HttpWebResponse)
                {
                    if (xmlResponse.StatusCode == HttpStatusCode.OK)
                    {

                        var settings = new XmlReaderSettings();
                        settings.DtdProcessing = DtdProcessing.Ignore; // Allow a DTD to be present in the XML, since it generally is there in a web page
                        settings.XmlResolver = null; // But don't follow the link to the DTD, which generally leads to a timeout
                        var reader = XmlReader.Create(xmlResponse.GetResponseStream(), settings);
                        try
                        {
                            return new XPathDocument(reader);
                        }
                        catch (XmlException ex)
                        {
                            // try to make the request again, in order to include the XML that failed in the error object
                            using (var xmlResponseForError = CreateRequest(request.RequestUri).GetResponse() as HttpWebResponse)
                            {
                                if (xmlResponseForError.StatusCode == HttpStatusCode.OK)
                                {
                                    using (var streamReader = new StreamReader(xmlResponseForError.GetResponseStream()))
                                    {
                                        ex.Data.Add("XML", streamReader.ReadToEnd());
                                        throw;
                                    }
                                }
                            }

                            // but if that doesn't work, at least include the remainder of the original stream
                            using (var streamReader = new StreamReader(xmlResponse.GetResponseStream()))
                            {
                                ex.Data.Add("XML", streamReader.ReadToEnd());
                                throw;
                            }
                        }
                    }
                    else
                    {
                        throw new WebException(String.Format(CultureInfo.InvariantCulture, "Request for XML data received an HTTP response of {0} {1}", xmlResponse.StatusCode, xmlResponse.StatusDescription));
                    }
                }
            }
            catch (WebException)
            {
                if (retries > 0)
                {
                    if (retries == 2) Thread.Sleep(1000);
                    if (retries == 1) Thread.Sleep(3000);
                    retries--;
                    return RequestXPath(request, retries);
                }
                throw;
            }
        }

        /// <summary>
        /// Requests XML data from a URL, transforms the response and returns the result in an XPathDocument
        /// </summary>
        /// <param name="requestUrl">The request URL.</param>
        /// <param name="xsltUrl">The XSLT URL.</param>
        /// <param name="xsltArguments">The XSLT arguments.</param>
        /// <returns></returns>
        public IXPathNavigable RequestXPath(Uri requestUrl, Uri xsltUrl, XsltArgumentList xsltArguments)
        {
            if (xsltUrl == null) throw new ArgumentNullException("xsltUrl");

            using (var ms = new MemoryStream())
            {
                var transform = new XslCompiledTransform();
                transform.Load(xsltUrl.ToString());
                transform.Transform(RequestXPath(requestUrl), xsltArguments, ms);
                ms.Position = 0;
                return new XPathDocument(ms);
            }
        }

        /// <summary>
        /// Requests data from an XML URL and loads the response into an XmlDocument
        /// </summary>
        /// <param name="requestUrl">The URI.</param>
        /// <exception cref="WebException">Thrown if XML response has an HTTP status other than 200 OK</exception>
        /// <returns></returns>
        public XmlDocument RequestXml(Uri requestUrl)
        {
            var xmlRequest = CreateRequest(requestUrl);
            using (var xmlResponse = xmlRequest.GetResponse() as HttpWebResponse)
            {
                if (xmlResponse.StatusCode == HttpStatusCode.OK)
                {
                    var xml = new XmlDocument();
                    xml.XmlResolver = null; // don't follow links, eg to DOCTYPE on W3C site, because it times out
                    xml.Load(xmlResponse.GetResponseStream());
                    return xml;
                }
                else
                {
                    throw new WebException(String.Format(CultureInfo.InvariantCulture, "Request for XML data received an HTTP response of {0} {1}", xmlResponse.StatusCode, xmlResponse.StatusDescription));
                }
            }
        }

        /// <summary>
        /// Requests XML data from a URL, transforms the response and returns the result in an XmlDocument
        /// </summary>
        /// <param name="requestUrl">The request URL.</param>
        /// <param name="xsltUrl">The XSLT URL.</param>
        /// <param name="xsltArguments">The XSLT arguments.</param>
        /// <returns></returns>
        public XmlDocument RequestXml(Uri requestUrl, Uri xsltUrl, XsltArgumentList xsltArguments)
        {
            if (xsltUrl == null) throw new ArgumentNullException("xsltUrl");

            using (var ms = new MemoryStream())
            {
                var transform = new XslCompiledTransform();
                transform.Load(xsltUrl.ToString());
                transform.Transform(RequestXPath(requestUrl), xsltArguments, ms);
                ms.Position = 0;

                var xml = new XmlDocument();
                xml.Load(ms);
                return xml;
            }
        }
    }
}
