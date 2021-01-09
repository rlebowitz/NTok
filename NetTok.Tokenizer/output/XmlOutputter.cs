using System;
using System.IO;
using System.Reflection.Metadata;
using Microsoft.Extensions.Logging;
using NetTok.Tokenizer.Annotate;
using NetTok.Tokenizer.Exceptions;

/*
 * JTok
 * A configurable tokenizer implemented in Java
 *
 * (C) 2003 - 2014  DFKI Language Technology Lab http://www.dfki.de/lt
 *   Author: Joerg Steffen, steffen@dfki.de
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

namespace NetTok.Tokenizer.output
{

	/// <summary>
	/// <seealso cref="XmlOutputter"/> provides static methods that return an XML presentation of an
	/// <seealso cref="IAnnotatedString"/>.
	/// 
	/// @author Joerg Steffen, DFKI
	/// </summary>
	public sealed class XmlOutputter
	{

	  /// <summary>
	  /// name of XML elements in the result that describe a document </summary>
	  public const string XML_DOCUMENT = "Document";

	  /// <summary>
	  /// name of XML elements in the result that describe a paragraph </summary>
	  public const string XML_PARAGRAPH = "p";

	  /// <summary>
	  /// name of XML elements in the result that describe a text unit; text units are contained in
	  /// paragraphs
	  /// </summary>
	  public const string XML_TEXT_UNIT = "tu";

	  /// <summary>
	  /// name of the XML attribute in {@code XML_TEXT_UNIT} that contains the text unit id </summary>
	  public const string ID_ATT = "id";

	  /// <summary>
	  /// name of XML elements in the result that describe a token; tokens are contained in text units
	  /// </summary>
	  public const string XML_TOKEN = "Token";

	  /// <summary>
	  /// name of the XML attribute in {@code XML_TOKEN} that contains the token image </summary>
	  public const string IMAGE_ATT = "string";

	  /// <summary>
	  /// name of the XML attribute in {@code XML_TOKEN} that contains the Penn Treebank token image if
	  /// it is any different than the regular surface string
	  /// </summary>
	  public const string PTB_ATT = "ptb";

	  /// <summary>
	  /// name of the XML attribute in {@code XML_TOKEN} that contains the token type </summary>
	  public const string TOK_TYPE_ATT = "type";

	  /// <summary>
	  /// name of the XML attribute in {@code XML_TOKEN} that contains the token offset </summary>
	  public const string OFFSET_ATT = "offset";

	  /// <summary>
	  /// name of the XML attribute in {@code XML_TOKEN} that contains the token length </summary>
	  public const string LENGTH_ATT = "length";

	  /// <summary>
	  /// the logger </summary>
	  private static readonly ILogger logger = LoggerFactory.getLogger(typeof(XmlOutputter));


	  // would create a new instance of {@link XmlOutputter}; not to be used
	  private XmlOutputter()
	  {

		// private constructor to enforce noninstantiability
	  }


	  /// <summary>
	  /// Creates an XML document from the given annotated string.
	  /// </summary>
	  /// <param name="input">
	  ///          the annotated string </param>
	  /// <returns> the XML document </returns>
	  /// <exception cref="ProcessingException">
	  ///              if an error occurs </exception>
	  public static Document createXmlDocument(IAnnotatedString input)
	  {

		// create result document
		Document doc = null;
		try
		{
		  DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
		  DocumentBuilder builder = factory.newDocumentBuilder();
		  doc = builder.newDocument();
		}
		catch (ParserConfigurationException pce)
		{
		  throw new ProcessingException(pce.LocalizedMessage, pce);
		}

		// create root element
		Element root = doc.createElement(XML_DOCUMENT);
		doc.appendChild(root);

		// init text unit counter
		int tuId = 0;

		// create paragraph element
		Element p = doc.createElement(XML_PARAGRAPH);
		// create text unit element
		Element tu = doc.createElement(XML_TEXT_UNIT);
		tu.setAttribute(ID_ATT, tuId + "");

		// iterate over tokens and create XML elements
		char c = input.setIndex(0);
		while (c != CharacterIterator.DONE)
		{

		  int tokenStart = input.GetRunStart(NTok.ClassAnnotation);
		  int tokenEnd = input.GetRunLimit(NTok.ClassAnnotation);
		  // check if c belongs to a token
		  if (null != input.GetAnnotation(NTok.ClassAnnotation))
		  {
			// get tag
			string type = (string)input.GetAnnotation(NTok.ClassAnnotation);
			if (null == type)
			{
			  throw new ProcessingException(string.Format("undefined class {0}", input.GetAnnotation(NTok.ClassAnnotation)));
			}
			// create new element
			Element xmlToken = doc.createElement(XML_TOKEN);
			// set attributes
			string image = input.Substring(tokenStart, tokenEnd - tokenStart);
			xmlToken.setAttribute(IMAGE_ATT, image);
			string ptbImage = Token.applyPtbFormat(image, type);
			if (null != ptbImage)
			{
			  xmlToken.setAttribute(PTB_ATT, ptbImage);
			}
			xmlToken.setAttribute(TOK_TYPE_ATT, type);
			xmlToken.setAttribute(OFFSET_ATT, tokenStart + "");
			xmlToken.setAttribute(LENGTH_ATT, image.Length + "");

			// check if token is first token of a paragraph or text unit
			if (null != input.GetAnnotation(NTok.BorderAnnotation))
			{
			  // add current text unit to paragraph and create new one
			  if (tu.hasChildNodes())
			  {
				p.appendChild(tu);
				tu = doc.createElement(XML_TEXT_UNIT);
				tuId++;
				tu.setAttribute(ID_ATT, tuId + "");
			  }
			}

			// check if token is first token of a paragraph
			if (input.GetAnnotation(NTok.BorderAnnotation) == NTok.PBorder)
			{
			  // add current paragraph to document and create new one
			  if (p.hasChildNodes())
			  {
				root.appendChild(p);
				p = doc.createElement(XML_PARAGRAPH);
			  }
			}

			// add token to text unit
			tu.appendChild(xmlToken);
		  }
		  // set iterator to next token
		  c = input.setIndex(tokenEnd);
		}
		// add last text units to paragraph
		if (tu.hasChildNodes())
		{
		  p.appendChild(tu);
		}
		// add last paragraph element to document
		if (p.hasChildNodes())
		{
		  root.appendChild(p);
		}

		// return document
		return doc;
	  }


	  /// <summary>
	  /// Creates an XML file from the given annotated string.
	  /// </summary>
	  /// <param name="input">
	  ///          the annotated string </param>
	  /// <param name="encoding">
	  ///          the encoding to use </param>
	  /// <param name="fileName">
	  ///          the name of the XML file </param>
	  /// <exception cref="ProcessingException">
	  ///              if an error occurs </exception>
	  public static void createXmlFile(IAnnotatedString input, string encoding, string fileName)
	  {

		// tokenize text
		Document doc = createXmlDocument(input);

		try
		{
		  // init writer for result
		  Writer @out = new StreamWriter(new FileStream(fileName, FileMode.Create, FileAccess.Write), encoding);
		  // use a transformer for output
		  Transformer transformer = TransformerFactory.newInstance().newTransformer();
		  transformer.setOutputProperty(OutputKeys.INDENT, "yes");
		  transformer.setOutputProperty(OutputKeys.ENCODING, encoding);
		  DOMSource source = new DOMSource(doc);
		  StreamResult result = new StreamResult(@out);
		  transformer.transform(source, result);
		  @out.close();
		}
		catch (TransformerException te)
		{
		  throw new ProcessingException(te.LocalizedMessage, te);
		}
		catch (IOException ioe)
		{
		  throw new ProcessingException(ioe.LocalizedMessage, ioe);
		}
	  }


	  /// <summary>
	  /// Creates an XML string from the given annotated string.
	  /// </summary>
	  /// <param name="input">
	  ///          the annotated string </param>
	  /// <returns> an XML String </returns>
	  /// <exception cref="ProcessingException">
	  ///              if an error occurs </exception>
	  public static string createXmlString(IAnnotatedString input)
	  {

		// tokenize text
		Document doc = createXmlDocument(input);

		// init output writer for result
		StringWriter @out = new StringWriter();

		// use a transformer for output
		try
		{
		  Transformer transformer = TransformerFactory.newInstance().newTransformer();
		  transformer.setOutputProperty(OutputKeys.INDENT, "yes");
		  DOMSource source = new DOMSource(doc);
		  StreamResult result = new StreamResult(@out);
		  transformer.transform(source, result);
		}
		catch (TransformerException te)
		{
		  throw new ProcessingException(te.LocalizedMessage, te);
		}

		// return result
		return @out.ToString();
	  }


	  /// <summary>
	  /// This main method must be used with two or three arguments:
	  /// <ul>
	  /// <li>a file name for the document to tokenize
	  /// <li>the language of the document
	  /// <li>an optional encoding to use (default is UTF-8)
	  /// </ul>
	  /// </summary>
	  /// <param name="args">
	  ///          the arguments </param>
	  public static void Main(string[] args)
	  {

		// check for correct arguments
		if ((args.Length != 2) && (args.Length != 3))
		{
		  Console.WriteLine("This method needs two arguments:\n" + "- a file name for the document to tokenize\n" + "- the language of the document\n" + "- an optional encoding to use (default is UTF-8)");
		  Environment.Exit(1);
		}

		// check encoding
		string encoding = "UTF-8";
		if (args.Length == 3)
		{
		  encoding = args[2];
		}

		string text = null;
		try
		{
		  // get text from file
		  text = FileTools.readFileAsString(new File(args[0]), encoding);
		}
		catch (IOException ioe)
		{
		  Console.WriteLine(ioe.ToString());
		  Console.Write(ioe.StackTrace);
		  Environment.Exit(1);
		}

		try
		{
		  // create new instance of JTok
		  NTok testTok = new NTok();

		  // tokenize text
		  IAnnotatedString result = testTok.Tokenize(text, args[1]);

		  // print result
		  Console.WriteLine(XmlOutputter.createXmlString(result));

		}
		catch (IOException e)
		{
		  logger.error(e.LocalizedMessage, e);
		}
	  }
	}

}