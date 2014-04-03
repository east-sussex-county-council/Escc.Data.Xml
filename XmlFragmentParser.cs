using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace EsccWebTeam.Data.Xml
{
    /// <summary>
    /// Utility methods to parse and work with fragments of XML
    /// </summary>
    public static class XmlFragmentParser
    {
        private static Collection<string> xhtmlMultiValuedAttributes = new Collection<string>(new string[] { "class", "rel", "rev" }); // and others?

        /// <summary>
        /// Gets a collection of XHTML attributes which can contain multiple values.
        /// </summary>
        /// <value>The XHTML multi-valued attributes.</value>
        public static Collection<string> XhtmlMultiValuedAttributes
        {
            get { return xhtmlMultiValuedAttributes; }
        }

        /// <summary>
        /// Parses the attributes string from inside an XML opening tag, even if they are not correctly quoted.
        /// </summary>
        /// <param name="attributes">The attributes from the tag as a single string.</param>
        /// <param name="attributeValues">Dictionary to store attributes.</param>
        public static void ParseAttributes(string attributes, Dictionary<string, string> attributeValues)
        {
            ParseAttributes(attributes, attributeValues, null, null, null, null);
        }
        /// <summary>
        /// Parses the attributes string from inside an XML opening tag, even if they are not correctly quoted.
        /// </summary>
        /// <param name="attributes">The attributes from the tag as a single string.</param>
        /// <param name="singleValueAttributes">Dictionary to store attributes which only have a single value.</param>
        /// <param name="multiValueAttributes">Dictionary to store attributes which can have multiple values.</param>
        /// <param name="splitValuesFor">Names of the attributes which can have multiple values.</param>
        /// <param name="splitValuesBy">Separators to split multiple values by.</param>
        /// <param name="unwantedAttributes">The names of any unwanted attributes, which will be discarded.</param>
        /// <remarks><see cref="List&lt;T&gt;"/> used for multi-valued attributes rather than <see cref="Collection&lt;T&gt;"/> because <see cref="Collection&lt;T&gt;"/> 
        /// was being treated as read-only. <see cref="List&lt;T&gt;"/> works, so it is used despite a recommendation from FxCop not to expose it.</remarks>
        public static void ParseAttributes(string attributes, Dictionary<string, string> singleValueAttributes, Dictionary<string, List<string>> multiValueAttributes, Collection<string> splitValuesFor, string[] splitValuesBy, Collection<string> unwantedAttributes)
        {
            int endPos;
            string attributeName;
            attributes = attributes.Trim();

            while (attributes.Length > 0)
            {
                int equalsPos = attributes.IndexOf("=");
                if (equalsPos < 0) break;
                switch (attributes.Substring(equalsPos + 1, 1))
                {
                    case "\"":
                        // Attribute is quoted using double quotes
                        endPos = attributes.IndexOf("\"", equalsPos + 2);
                        attributeName = attributes.Substring(0, equalsPos).ToLowerInvariant();
                        if (splitValuesFor != null && splitValuesFor.Contains(attributeName))
                        {
                            // Combine multiple uses of the same attribute in the same tag, because multiple uses of the same attribute is not well-formed XML
                            string[] attributeValues = attributes.Substring(equalsPos + 2, endPos - equalsPos - 2).Split(splitValuesBy, StringSplitOptions.RemoveEmptyEntries);
                            if (multiValueAttributes.ContainsKey(attributeName))
                            {
                                foreach (string extraValue in attributeValues) multiValueAttributes[attributeName].Add(extraValue);
                            }
                            else
                            {
                                multiValueAttributes.Add(attributeName, new List<string>(attributeValues));
                            }
                        }
                        else
                        {
                            // This attribute can only have a single value, so if multiple uses of the same attribute keep only the first one, 
                            // because multiple uses of the same attribute is not well-formed XML
                            if (!singleValueAttributes.ContainsKey(attributeName))
                            {
                                singleValueAttributes.Add(attributeName, attributes.Substring(equalsPos + 2, endPos - equalsPos - 2));
                            }
                        }
                        attributes = attributes.Substring(endPos + 1).Trim();
                        break;
                    case "'":
                        // Attribute is quoted using single quotes
                        endPos = attributes.IndexOf("'", equalsPos + 2);
                        attributeName = attributes.Substring(0, equalsPos).ToLowerInvariant();
                        if (splitValuesFor != null && splitValuesFor.Contains(attributeName))
                        {
                            // Combine multiple uses of the same attribute in the same tag, because multiple uses of the same attribute is not well-formed XML
                            string[] attributeValues = attributes.Substring(equalsPos + 2, endPos - equalsPos - 2).Split(splitValuesBy, StringSplitOptions.RemoveEmptyEntries);
                            if (multiValueAttributes.ContainsKey(attributeName))
                            {
                                foreach (string extraValue in attributeValues) multiValueAttributes[attributeName].Add(extraValue);
                            }
                            else
                            {
                                multiValueAttributes.Add(attributeName, new List<string>(attributeValues));
                            }
                        }
                        else
                        {
                            // This attribute can only have a single value, so if multiple uses of the same attribute keep only the first one, 
                            // because multiple uses of the same attribute is not well-formed XML
                            if (!singleValueAttributes.ContainsKey(attributeName))
                            {
                                singleValueAttributes.Add(attributeName, attributes.Substring(equalsPos + 2, endPos - equalsPos - 2));
                            }
                        }
                        attributes = attributes.Substring(endPos + 1).Trim();
                        break;
                    default:
                        // Attribute is not quoted (for example, SharePoint code mangling) so ends with next space or end of string
                        endPos = attributes.IndexOf(" ", equalsPos + 1);
                        if (endPos == -1) endPos = attributes.Length;
                        attributeName = attributes.Substring(0, equalsPos).ToLowerInvariant();
                        if (splitValuesFor != null && splitValuesFor.Contains(attributeName))
                        {
                            // Combine multiple uses of the same attribute in the same tag, because multiple uses of the same attribute is not well-formed XML
                            string[] attributeValues = attributes.Substring(equalsPos + 1, endPos - equalsPos - 1).Split(splitValuesBy, StringSplitOptions.RemoveEmptyEntries);
                            if (multiValueAttributes.ContainsKey(attributeName))
                            {
                                foreach (string extraValue in attributeValues) multiValueAttributes[attributeName].Add(extraValue);
                            }
                            else
                            {
                                multiValueAttributes.Add(attributeName, new List<string>(attributeValues));
                            }
                        }
                        else
                        {
                            // This attribute can only have a single value, so if multiple uses of the same attribute keep only the first one, 
                            // because multiple uses of the same attribute is not well-formed XML
                            if (!singleValueAttributes.ContainsKey(attributeName))
                            {
                                singleValueAttributes.Add(attributeName, attributes.Substring(equalsPos + 1, endPos - equalsPos - 1));
                            }
                        }
                        attributes = attributes.Substring(endPos).Trim();
                        break;
                }
            }

            // Ditch unwanted attributes
            if (unwantedAttributes != null)
            {
                foreach (string unwantedAttribute in unwantedAttributes)
                {
                    if (singleValueAttributes.ContainsKey(unwantedAttribute)) singleValueAttributes.Remove(unwantedAttribute);
                    else if (multiValueAttributes.ContainsKey(unwantedAttribute)) multiValueAttributes.Remove(unwantedAttribute);
                }
            }
        }

        /// <summary>
        /// Rebuilds an XML opening tag which was parsed by <see cref="ParseAttributes(string, Dictionary&lt;string, string&gt;, Dictionary&lt;string, List&lt;string&gt;&gt;, Collection&lt;string&gt;, string[], Collection&lt;string&gt;)"/>
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="singleValueAttributes">The single value attributes.</param>
        /// <param name="multiValueAttributes">The multi value attributes.</param>
        /// <returns>XML opening tag</returns>
        /// <remarks><see cref="List&lt;T&gt;"/> used for multi-valued attributes rather than <see cref="Collection&lt;T&gt;"/> because <see cref="Collection&lt;T&gt;"/> 
        /// was being treated as read-only. <see cref="List&lt;T&gt;"/> works, so it is used despite a recommendation from FxCop not to expose it.</remarks>
        public static string RebuildTag(string tagName, Dictionary<string, string> singleValueAttributes, Dictionary<string, List<string>> multiValueAttributes)
        {
            return RebuildTag(tagName, singleValueAttributes, multiValueAttributes, null, false);
        }

        /// <summary>
        /// Rebuilds an XML opening tag which was parsed by <see cref="ParseAttributes(string, Dictionary&lt;string, string&gt;, Dictionary&lt;string, List&lt;string&gt;&gt;, Collection&lt;string&gt;, string[], Collection&lt;string&gt;)"/>
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="singleValueAttributes">The single value attributes.</param>
        /// <param name="multiValueAttributes">The multi value attributes.</param>
        /// <param name="restrictToAttributes">The only attributes to use when reassembling tag.</param>
        /// <param name="isEmptyElement">if set to <c>true</c> [is empty element].</param>
        /// <returns>XML opening tag</returns>
        /// <remarks><see cref="List&lt;T&gt;"/> used for multi-valued attributes rather than <see cref="Collection&lt;T&gt;"/> because <see cref="Collection&lt;T&gt;"/>
        /// was being treated as read-only. <see cref="List&lt;T&gt;"/> works, so it is used despite a recommendation from FxCop not to expose it.</remarks>
        public static string RebuildTag(string tagName, Dictionary<string, string> singleValueAttributes, Dictionary<string, List<string>> multiValueAttributes, Collection<string> restrictToAttributes, bool isEmptyElement)
        {
            StringBuilder tagBuilder = new StringBuilder();
            tagBuilder.Append("<");
            tagBuilder.Append(tagName.ToLowerInvariant());
            foreach (string key in singleValueAttributes.Keys)
            {
                if (restrictToAttributes != null && !restrictToAttributes.Contains(key)) continue;

                // Ditch empty attributes, save the rest
                if (singleValueAttributes[key].Length > 0)
                {
                    tagBuilder.Append(" ").Append(key).Append("=\"").Append(singleValueAttributes[key]).Append("\"");
                }
            }
            foreach (string key in multiValueAttributes.Keys)
            {
                if (restrictToAttributes != null && !restrictToAttributes.Contains(key)) continue;

                // Ditch empty attributes, save the rest
                if (multiValueAttributes[key].Count > 0)
                {
                    string[] multiValueAttributesArray = new string[multiValueAttributes[key].Count];
                    multiValueAttributes[key].CopyTo(multiValueAttributesArray, 0);
                    tagBuilder.Append(" ").Append(key).Append("=\"").Append(String.Join(" ", multiValueAttributesArray)).Append("\"");
                }
            }
            if (isEmptyElement) tagBuilder.Append(" /");
            tagBuilder.Append(">");
            return tagBuilder.ToString();
        }

        /// <summary>
        /// Rebuilds an XML element which was parsed by <see cref="ParseAttributes(string, Dictionary&lt;string, string&gt;, Dictionary&lt;string, List&lt;string&gt;&gt;, Collection&lt;string&gt;, string[], Collection&lt;string&gt;)"/>
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="singleValueAttributes">The single value attributes.</param>
        /// <param name="multiValueAttributes">The multi value attributes.</param>
        /// <param name="innerXml">The inner XML.</param>
        /// <returns>XML tag</returns>
        /// <remarks><see cref="List&lt;T&gt;"/> used for multi-valued attributes rather than <see cref="Collection&lt;T&gt;"/> because <see cref="Collection&lt;T&gt;"/> 
        /// was being treated as read-only. <see cref="List&lt;T&gt;"/> works, so it is used despite a recommendation from FxCop not to expose it.</remarks>
        public static string RebuildElement(string tagName, Dictionary<string, string> singleValueAttributes, Dictionary<string, List<string>> multiValueAttributes, string innerXml)
        {
            return RebuildTag(tagName, singleValueAttributes, multiValueAttributes) + innerXml + "</" + tagName + ">";
        }
    }
}
