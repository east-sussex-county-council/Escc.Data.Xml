using System;
using System.Net;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace Escc.Net
{
    /// <summary>
    /// Helper class for requesting online resources, with specialised methods for XML data
    /// </summary>
    public interface IHttpRequestClient
    {
        /// <summary>
        /// Gets or sets the provider for configuring a proxy server.
        /// </summary>
        /// <value>
        /// The proxy provider.
        /// </value>
        IProxyProvider ProxyProvider { get; set; }

        /// <summary>
        /// Creates a new <see cref="HttpWebRequest"/> for the specified URI, with proxy access configured.
        /// </summary>
        /// <param name="requestUri">The request URI.</param>
        /// <returns></returns>
        HttpWebRequest CreateRequest(Uri requestUri);

        /// <summary>
        /// Requests an URL and loads the response into a string
        /// </summary>
        /// <param name="requestUrl">The URL.</param>
        /// <exception cref="WebException">Thrown if response has an HTTP status other than 200 OK</exception>
        /// <returns></returns>
        string RequestString(Uri requestUrl);

        /// <summary>
        /// Requests XML data from a URL, transforms the response and returns the result as a string
        /// </summary>
        /// <param name="requestUrl">The request URL.</param>
        /// <param name="xsltUrl">The XSLT URL.</param>
        /// <param name="xsltArguments">The XSLT arguments.</param>
        /// <returns></returns>
        string RequestString(Uri requestUrl, Uri xsltUrl, XsltArgumentList xsltArguments);

        /// <summary>
        /// Requests XML data from a URL and loads the response into an XPathDocument
        /// </summary>
        /// <param name="requestUrl">The URL.</param>
        /// <exception cref="WebException">Thrown if XML response has an HTTP status other than 200 OK</exception>
        /// <returns></returns>
        IXPathNavigable RequestXPath(Uri requestUrl);

        /// <summary>
        /// Requests XML data from a URL and loads the response into an XPathDocument
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        /// <exception cref="WebException">Thrown if XML response has an HTTP status other than 200 OK</exception>
        IXPathNavigable RequestXPath(WebRequest request);

        /// <summary>
        /// Requests XML data from a URL and loads the response into an XPathDocument
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="retries">Amount of times to retry a query. Maximum is 2.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">request</exception>
        /// <exception cref="System.Net.WebException"></exception>
        /// <exception cref="WebException">Thrown if XML response has an HTTP status other than 200 OK</exception>
        IXPathNavigable RequestXPath(WebRequest request, int retries);

        /// <summary>
        /// Requests XML data from a URL, transforms the response and returns the result in an XPathDocument
        /// </summary>
        /// <param name="requestUrl">The request URL.</param>
        /// <param name="xsltUrl">The XSLT URL.</param>
        /// <param name="xsltArguments">The XSLT arguments.</param>
        /// <returns></returns>
        IXPathNavigable RequestXPath(Uri requestUrl, Uri xsltUrl, XsltArgumentList xsltArguments);

        /// <summary>
        /// Requests data from an XML URL and loads the response into an XmlDocument
        /// </summary>
        /// <param name="requestUrl">The URI.</param>
        /// <exception cref="WebException">Thrown if XML response has an HTTP status other than 200 OK</exception>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode")]
        XmlDocument RequestXml(Uri requestUrl);

        /// <summary>
        /// Requests XML data from a URL, transforms the response and returns the result in an XmlDocument
        /// </summary>
        /// <param name="requestUrl">The request URL.</param>
        /// <param name="xsltUrl">The XSLT URL.</param>
        /// <param name="xsltArguments">The XSLT arguments.</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode")]
        XmlDocument RequestXml(Uri requestUrl, Uri xsltUrl, XsltArgumentList xsltArguments);
    }
}