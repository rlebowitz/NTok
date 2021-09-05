using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using NetTok.Tokenizer.Annotate;
using NetTok.Tokenizer.Exceptions;

/*
 * NTok
 * A configurable tokenizer implemented in C# based on the Java JTok tokenizer.
 *
 * (c) 2003 - 2014  DFKI Language Technology Lab http://www.dfki.de/lt
 *   Author: Joerg Steffen, steffen@dfki.de
 *
 * (c) 2021 - Finaltouch IT LLC
 *   Author:  Robert Lebowitz, lebowitz@finaltouch.com
 *
 *   This program is free software; you can redistribute it and/or
 *   modify it under the terms of the GNU Lesser General Public
 *   License as published by the Free Software Foundation; either
 *   version 2.1 of the License, or (at your option) any later version.
 *
 *   This program is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *   Lesser General Public License for more details.
 *
 *   You should have received a copy of the GNU Lesser General Public
 *   License along with this library; if not, write to the Free Software
 *   Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */

namespace NetTok.Tokenizer.Output
{
    /// <summary>
    ///     XmlOutputter provides static methods that return an XML presentation of an IAnnotatedString.
    ///     @author Joerg Steffen, DFKI, Robert J Lebowitz, Finaltouch IT LLC
    /// </summary>
    public static class XmlOutputter
    {
        /// <summary>
        ///     name of XML elements in the result that describe a document
        /// </summary>
        public const string XMLDocument = "Document";

        /// <summary>
        ///     name of XML elements in the result that describe a paragraph
        /// </summary>
        public const string XMLParagraph = "p";

        /// <summary>
        ///     name of XML elements in the result that describe a text unit; text units are contained in
        ///     paragraphs
        /// </summary>
        public const string XMLTextUnit = "tu";

        /// <summary>
        ///     name of the XML attribute in {@code XML_TEXT_UNIT} that contains the text unit id
        /// </summary>
        public const string IdAttribute = "id";

        /// <summary>
        ///     name of XML elements in the result that describe a token; tokens are contained in text units
        /// </summary>
        public const string XMLToken = "Token";

        /// <summary>
        ///     name of the XML attribute in {@code XML_TOKEN} that contains the token image
        /// </summary>
        public const string ImageAttribute = "string";

        /// <summary>
        ///     name of the XML attribute in {@code XML_TOKEN} that contains the Penn Treebank token image if
        ///     it is any different than the regular surface string
        /// </summary>
        public const string PennTreeBankAttribute = "ptb";

        /// <summary>
        ///     name of the XML attribute in {@code XML_TOKEN} that contains the token type
        /// </summary>
        public const string TokenTypeAttribute = "type";

        /// <summary>
        ///     name of the XML attribute in {@code XML_TOKEN} that contains the token offset
        /// </summary>
        public const string OffsetAttribute = "offset";

        /// <summary>
        ///     name of the XML attribute in {@code XML_TOKEN} that contains the token length
        /// </summary>
        public const string LengthAttribute = "length";

        /// <summary>
        ///     Creates an XML document from the given annotated string.
        /// </summary>
        /// <param name="input">The annotated string.</param>
        /// <returns>The XDocument.</returns>
        /// <exception cref="ProcessingException">if an error occurs </exception>
        public static XDocument CreateXmlDocument(IAnnotatedString input)
        {
            // create result document
            var doc = new XDocument(new XElement(XMLDocument));
            var root = doc.Root;
            // init text unit counter
            var tuId = 0;
            // create paragraph element
            var p = new XElement(XMLParagraph);
            // create text unit element
            var tu = new XElement(XMLTextUnit, new XAttribute(IdAttribute, Convert.ToString(tuId)));
            // iterate over tokens and create XML elements
            var c = input.SetIndex(0);
            while (c != default)
            {
                var tokenStart = input.GetRunStart(NTok.ClassAnnotation);
                var tokenEnd = input.GetRunLimit(NTok.ClassAnnotation);
                // check if c belongs to a token
                if (null != input.GetAnnotation(NTok.ClassAnnotation))
                {
                    // get tag
                    var type = (string) input.GetAnnotation(NTok.ClassAnnotation);
                    if (null == type)
                    {
                        throw new ProcessingException($"Undefined class {input.GetAnnotation(NTok.ClassAnnotation)}");
                    }

                    var image = input.Substring(tokenStart, tokenEnd);
                    var ptbImage = Token.ApplyPennTreeBankFormat(image, type);
                    // create new element
                    var xmlToken = new XElement(XMLToken,
                        new XAttribute(ImageAttribute, image),
                        new XAttribute(TokenTypeAttribute, type),
                        new XAttribute(OffsetAttribute, Convert.ToString(tokenStart)),
                        new XAttribute(LengthAttribute, Convert.ToString(image.Length))
                    );
                    if (ptbImage != null)
                    {
                        xmlToken.Add(ptbImage);
                    }

                    // check if token is first token of a paragraph or text unit
                    if (null != input.GetAnnotation(NTok.BorderAnnotation))
                    {
                        // add current text unit to paragraph and create new one
                        if (tu.HasElements)
                        {
                            p.Add(tu);
                            tuId++;
                            tu = new XElement(XMLTextUnit,
                                new XAttribute(IdAttribute, Convert.ToString(tuId)));
                        }
                    }

                    // check if token is first token of a paragraph
                    if (NTok.PBorder.Equals((string) input.GetAnnotation(NTok.BorderAnnotation)))
                    {
                        // add current paragraph to document and create new one
                        if (p.HasElements)
                        {
                            root?.Add(p);
                            p = new XElement(XMLParagraph);
                        }
                    }

                    // add token to text unit
                    tu.Add(xmlToken);
                }

                // set iterator to next token
                c = input.SetIndex(tokenEnd);
            }

            // add last text units to paragraph
            if (tu.HasElements)
            {
                p.Add(tu);
            }

            // add last paragraph element to document
            if (p.HasElements)
            {
                root?.Add(p);
            }

            return doc;
        }

        /// <summary>
        ///     Creates an XML file from the given annotated string.
        /// </summary>
        /// <param name="input">The annotated string.</param>
        /// <param name="encoding">The encoding to use.</param>
        /// <param name="fileName">The name of the XML file.</param>
        public static void CreateXmlFile(IAnnotatedString input, string encoding, string fileName)
        {
            // tokenize text
            var doc = CreateXmlDocument(input);
            // init writer for result
            using var writer = new StreamWriter(new FileStream(fileName, FileMode.Create, FileAccess.Write));
            doc.WriteTo(new XmlTextWriter(writer) {Formatting = Formatting.Indented});
        }


        /// <summary>
        ///     Creates an XML string from the given annotated string.
        /// </summary>
        /// <param name="input">The annotated string.</param>
        /// <returns>An XML String.</returns>
        public static string CreateXmlString(IAnnotatedString input)
        {
            var doc = CreateXmlDocument(input);
            using var writer = new StringWriter();
            doc.WriteTo(new XmlTextWriter(writer) {Formatting = Formatting.Indented});
            return writer.ToString();
        }
    }
}