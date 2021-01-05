using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace NetTok.Tokenizer
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Returns the first ancestral XElement with the specified XName value.
        /// </summary>
        /// <param name="element">The specified ancestral XElement.</param>
        /// <param name="xName">The specified XName value.</param>
        /// <returns>The first ancestor if it exists, otherwise null.</returns>
        public static XElement Ancestor(this XElement element, string xName)
        {
            return element.Ancestors(xName).FirstOrDefault();
        }

        public static string TagName(this XElement element)
        {
            return element.Name.LocalName;
        }
    }
}
