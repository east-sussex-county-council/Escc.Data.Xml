using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Threading;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace EsccWebTeam.Data.Xml
{
    /// <summary>
    /// Helper class for requesting online resources as XML 
    /// </summary>
    /// <remarks>
    /// <para>If required, the account used for the proxy server should be specified in web.config, as follows:</para>
    /// <example>
    /// &lt;configuration&gt;
    ///   &lt;configSections&gt;
    ///       &lt;sectionGroup name=&quot;EsccWebTeam.Data.Xml&quot;&gt;
    ///           &lt;section name=&quot;Proxy&quot; type=&quot;System.Configuration.NameValueSectionHandler, System, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089&quot;/&gt;
    ///       &lt;/sectionGroup&gt;
    ///   &lt;/configSections&gt;
    /// 
    ///     &lt;EsccWebTeam.Data.Xml&gt;
    ///         &lt;Proxy&gt;
    ///             &lt;add key=&quot;Server&quot; value=&quot;proxy server IP&quot; /&gt;
    ///             &lt;add key=&quot;User&quot; value=&quot;username&quot; /&gt;
    ///             &lt;add key=&quot;Domain&quot; value=&quot;domain&quot; /&gt;
    ///             &lt;add key=&quot;Password&quot; value=&quot;password&quot; /&gt;
    ///         &lt;/Proxy&gt;
    ///     &lt;/EsccWebTeam.Data.Xml&gt;
    /// &lt;/configuration&gt;
    /// </example>
    /// </remarks>
    public static class XmlHttpRequest
    {
        /// <summary>
        /// Creates a new HttpWebRequest for the specified URI, with proxy access configured.
        /// </summary>
        /// <param name="requestUri">The request URI.</param>
        /// <returns></returns>
        public static HttpWebRequest Create(Uri requestUri)
        {
            HttpWebRequest xmlRequest = (HttpWebRequest)WebRequest.Create(requestUri);
            xmlRequest.UserAgent = "EsccWebTeam.Data.Xml"; // Some apps require a user-agent to be present
            IWebProxy proxy = CreateProxy();
            if (proxy != null)
            {
                xmlRequest.Proxy = proxy;
                xmlRequest.Credentials = proxy.Credentials;
            }
            else
            {
                xmlRequest.Credentials = CredentialCache.DefaultCredentials;
            }
            return xmlRequest;
        }

        /// <summary>
        /// Loads settings from web.config which allow requests to go out through the ESCC proxy server
        /// </summary>
        /// <returns>Configured proxy object, or <c>null</c> if settings not found</returns>
        public static IWebProxy CreateProxy()
        {
            if (ConfigurationManager.GetSection("EsccWebTeam.Data.Xml/Proxy") == null) return null; // if no proxy user configured, don't return a proxy
            return new EsccProxy();
        }

        /// <summary>
        /// Requests an XML URI and loads the response into a string
        /// </summary>
        /// <param name="requestUri">The URI.</param>
        /// <exception cref="WebException">Thrown if XML response has an HTTP status other than 200 OK</exception>
        /// <returns></returns>
        public static string RequestString(Uri requestUri)
        {
            string xml = String.Empty;
            HttpWebRequest xmlRequest = XmlHttpRequest.Create(requestUri);
            using (HttpWebResponse xmlResponse = xmlRequest.GetResponse() as HttpWebResponse)
            {
                if (xmlResponse.StatusCode == HttpStatusCode.OK)
                {
                    using (StreamReader reader = new StreamReader(xmlResponse.GetResponseStream()))
                    {
                        xml = reader.ReadToEnd();
                    }
                }
                else
                {
                    throw new WebException(String.Format("Request for XML data received an HTTP response of {0} {1}", xmlResponse.StatusCode, xmlResponse.StatusDescription));
                }
            }
            return xml;
        }

        /// <summary>
        /// Requests an XML URI, transforms the response and returns the result as a string
        /// </summary>
        /// <param name="requestUri">The request URI.</param>
        /// <param name="xsltUri">The XSLT URI.</param>
        /// <param name="xsltArguments">The XSLT arguments.</param>
        /// <returns></returns>
        public static string RequestString(Uri requestUri, Uri xsltUri, XsltArgumentList xsltArguments)
        {
            string transformResult = String.Empty;
            using (MemoryStream ms = new MemoryStream())
            {
                XslCompiledTransform transform = new XslCompiledTransform();
                transform.Load(xsltUri.ToString());
                transform.Transform(RequestXPath(requestUri), xsltArguments, ms);
                ms.Position = 0;
                using (StreamReader reader = new StreamReader(ms))
                {
                    transformResult = reader.ReadToEnd();
                }
            }
            return transformResult;
        }

        /// <summary>
        /// Requests a URI and loads the response into an XPathDocument
        /// </summary>
        /// <param name="requestUri">The URI.</param>
        /// <exception cref="WebException">Thrown if XML response has an HTTP status other than 200 OK</exception>
        /// <returns></returns>
        public static XPathDocument RequestXPath(Uri requestUri)
        {
            if (requestUri == null) throw new ArgumentNullException("requestUri");
            return (XPathDocument)RequestXPath(XmlHttpRequest.Create(requestUri));
        }

        /// <summary>
        /// Requests a URI and loads the response into an XPathDocument
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        /// <exception cref="WebException">Thrown if XML response has an HTTP status other than 200 OK</exception>
        public static IXPathNavigable RequestXPath(WebRequest request)
        {
            return RequestXPath(request, 0);
        }

        /// <summary>
        /// Requests a URI and loads the response into an XPathDocument
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="retries">Amount of times to retry a query. Maximum is 2.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">request</exception>
        /// <exception cref="System.Net.WebException"></exception>
        /// <exception cref="WebException">Thrown if XML response has an HTTP status other than 200 OK</exception>
        public static IXPathNavigable RequestXPath(WebRequest request, int retries)
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
                        settings.ProhibitDtd = false; // Allow a DTD to be present in the XML, since it generally is there in a web page
                        settings.XmlResolver = null; // But don't follow the link to the DTD, which generally leads to a timeout
                        XmlReader reader = XmlReader.Create(xmlResponse.GetResponseStream(), settings);
                        try
                        {
                            return new XPathDocument(reader);
                        }
                        catch (XmlException ex)
                        {
                            // try to make the request again, in order to include the XML that failed in the error object
                            using (var xmlResponseForError = XmlHttpRequest.Create(request.RequestUri).GetResponse() as HttpWebResponse)
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
                        throw new WebException(String.Format("Request for XML data received an HTTP response of {0} {1}", xmlResponse.StatusCode, xmlResponse.StatusDescription));
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
        /// Requests a URI, transforms the response and returns the result in an XPathDocument
        /// </summary>
        /// <param name="requestUri">The request URI.</param>
        /// <param name="xsltUri">The XSLT URI.</param>
        /// <param name="xsltArguments">The XSLT arguments.</param>
        /// <returns></returns>
        public static XPathDocument RequestXPath(Uri requestUri, Uri xsltUri, XsltArgumentList xsltArguments)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                XslCompiledTransform transform = new XslCompiledTransform();
                transform.Load(xsltUri.ToString());
                transform.Transform(RequestXPath(requestUri), xsltArguments, ms);
                ms.Position = 0;
                return new XPathDocument(ms);
            }
        }

        /// <summary>
        /// Requests a URI and loads the response into an XmlDocument
        /// </summary>
        /// <param name="requestUri">The URI.</param>
        /// <exception cref="WebException">Thrown if XML response has an HTTP status other than 200 OK</exception>
        /// <returns></returns>
        public static XmlDocument RequestXml(Uri requestUri)
        {
            HttpWebRequest xmlRequest = XmlHttpRequest.Create(requestUri);
            using (HttpWebResponse xmlResponse = xmlRequest.GetResponse() as HttpWebResponse)
            {
                if (xmlResponse.StatusCode == HttpStatusCode.OK)
                {
                    XmlDocument xml = new XmlDocument();
                    xml.XmlResolver = null; // don't follow links, eg to DOCTYPE on W3C site, because it times out
                    xml.Load(xmlResponse.GetResponseStream());
                    return xml;
                }
                else
                {
                    throw new WebException(String.Format("Request for XML data received an HTTP response of {0} {1}", xmlResponse.StatusCode, xmlResponse.StatusDescription));
                }
            }
        }

        /// <summary>
        /// Requests a URI, transforms the response and returns the result in an XmlDocument
        /// </summary>
        /// <param name="requestUri">The request URI.</param>
        /// <param name="xsltUri">The XSLT URI.</param>
        /// <param name="xsltArguments">The XSLT arguments.</param>
        /// <returns></returns>
        public static XmlDocument RequestXml(Uri requestUri, Uri xsltUri, XsltArgumentList xsltArguments)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                XslCompiledTransform transform = new XslCompiledTransform();
                transform.Load(xsltUri.ToString());
                transform.Transform(RequestXPath(requestUri), xsltArguments, ms);
                ms.Position = 0;

                XmlDocument xml = new XmlDocument();
                xml.Load(ms);
                return xml;
            }
        }
    }
}
