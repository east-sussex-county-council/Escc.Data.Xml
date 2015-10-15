using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Escc.Net
{
    /// <summary>
    /// Convert an object between its original type and its proxy representation
    /// </summary>
    /// <typeparam name="DataObjectType">The original <see cref="System.Type"/>.</typeparam>
    /// <typeparam name="ProxyObjectType">The <see cref="System.Type"/> of the proxy object.</typeparam>
    interface IProxyObjectConverter<DataObjectType, ProxyObjectType>
    {
        /// <summary>
        /// Converts a proxy object to its original type
        /// </summary>
        /// <param name="proxyObject">The proxy object</param>
        /// <returns>An instance of the original type, recreated from the data contained in the proxy</returns>
        /// <seealso cref="ConvertOriginalTypeToProxy"/>
        DataObjectType ConvertProxyToOriginalType(ProxyObjectType proxyObject);

        /// <summary>
        /// Converts an original type to its proxy representation.
        /// </summary>
        /// <param name="dataObject">The data object.</param>
        /// <returns>Proxy object</returns>
        /// <seealso cref="ConvertProxyToOriginalType"/>
        ProxyObjectType ConvertOriginalTypeToProxy(DataObjectType dataObject);
    }
}
