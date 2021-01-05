using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NetTok.Tokenizer.exceptions;
using NetTok.Tokenizer.regexp;

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

namespace NetTok.Tokenizer
{

	using Element = org.w3c.dom.Element;
	using Node = org.w3c.dom.Node;
	using NodeList = org.w3c.dom.NodeList;

	using InitializationException = InitializationException;
	using ProcessingException = ProcessingException;
	using DkBricsRegExpFactory = DkBricsRegExpFactory;
	using Match = Match;
	using RegExp = RegExp;
	using RegExpFactory = RegExpFactory;

	/// <summary>
	/// Abstract class that provides common methods to manage the content of description files.
	/// 
	/// @author Joerg Steffen, DFKI
	/// </summary>
	public abstract class Description
	{

	  /// <summary>
	  /// name of the element with the definitions in the description files </summary>
	  protected internal const string DEFS = "DEFINITIONS";

	  /// <summary>
	  /// attribute of a definition element that contains the regular expression </summary>
	  protected internal const string DEF_REGEXP = "regexp";

	  /// <summary>
	  /// attribute of a definition or list element that contains the class name </summary>
	  protected internal const string DEF_CLASS = "class";

	  /// <summary>
	  /// name of the element with the lists in the description files </summary>
	  protected internal const string LISTS = "LISTS";

	  /// <summary>
	  /// attribute of a list element that point to the list file </summary>
	  protected internal const string LIST_FILE = "file";

	  /// <summary>
	  /// attribute of a list element that contains the encoding of the list file </summary>
	  protected internal const string LIST_ENCODING = "encoding";

	  /// <summary>
	  /// name of the element with the rules in the description files </summary>
	  protected internal const string RULES = "RULES";

	  /// <summary>
	  /// factory for creating regular expressions </summary>
	  protected internal static readonly RegExpFactory FACTORY = new DkBricsRegExpFactory();

	  /// <summary>
	  /// single line in descriptions that marks the start of the lists section </summary>
	  protected internal const string LISTS_MARKER = "LISTS:";

	  /// <summary>
	  /// single line in descriptions that marks the start of the definitions section </summary>
	  protected internal const string DEFS_MARKER = "DEFINITIONS:";

	  /// <summary>
	  /// single line in descriptions that marks the start of the rules section </summary>
	  protected internal const string RULES_MARKER = "RULES:";


	  // regular expression for matching references used in regular expressions of config files
	  private static readonly RegExp REF_MATCHER = FACTORY.createRegExp("\\<[A-Za-z0-9_]+\\>");


	  /// <summary>
	  /// Maps a class name to a regular expression that matches all tokens of this class. The regular
	  /// expression is build as a disjunction of the regular expressions used in the definitions. If a
	  /// rule matches expressions from more than one class, this is used to identify the class.
	  /// </summary>
	  protected internal IDictionary<string, RegExp> definitionsMap;

	  /// <summary>
	  /// Maps the rule names to regular expressions that match the tokens as described by the rule.
	  /// </summary>
	  protected internal IDictionary<string, RegExp> rulesMap;

	  /// <summary>
	  /// Maps regular expressions of rules to class names of the matched expression. This is used for
	  /// rules that only match expressions that all have the same class.
	  /// </summary>
	  protected internal IDictionary<RegExp, string> regExpMap;

	  /// <summary>
	  /// Maps a class to a set containing members of this class.
	  /// </summary>
	  protected internal IDictionary<string, ISet<string>> classMembersMap;


	  /// <returns> the definitions map </returns>
	  protected internal virtual IDictionary<string, RegExp> DefinitionsMap
	  {
		  get
		  {
    
			return this.definitionsMap;
		  }
		  set
		  {
    
			this.definitionsMap = value;
		  }
	  }




	  /// <returns> the rules map </returns>
	  protected internal virtual IDictionary<string, RegExp> RulesMap
	  {
		  get
		  {
    
			return this.rulesMap;
		  }
		  set
		  {
    
			this.rulesMap = value;
		  }
	  }




	  /// <returns> the regular expressions map </returns>
	  protected internal virtual IDictionary<RegExp, string> RegExpMap
	  {
		  get
		  {
    
			return this.regExpMap;
		  }
		  set
		  {
    
			this.regExpMap = value;
		  }
	  }




	  /// <returns> the class members map </returns>
	  protected internal virtual IDictionary<string, ISet<string>> ClassMembersMap
	  {
		  get
		  {
    
			return this.classMembersMap;
		  }
		  set
		  {
    
			this.classMembersMap = value;
		  }
	  }




	  /// <summary>
	  /// Returns the first child element of the given element with the given name. If no such child
	  /// exists, returns {@code null}.
	  /// </summary>
	  /// <param name="ele">
	  ///          the parent element </param>
	  /// <param name="childName">
	  ///          the child name </param>
	  /// <returns> the first child element with the specified name or {@code null} if no such child exists </returns>
	  protected internal virtual Element getChild(Element ele, string childName)
	  {

		NodeList children = ele.ChildNodes;
		for (int i = 0, iMax = children.Length; i < iMax; i++)
		{
		  Node oneChild = children.item(i);
		  if ((oneChild is Element) && ((Element)oneChild).TagName.Equals(childName))
		  {
			return (Element)oneChild;
		  }
		}
		return null;
	  }


	  /// <summary>
	  /// Reads the macro configuration from the given path and adds it to the given map.
	  /// </summary>
	  /// <param name="macroPath">
	  ///          path to the config file </param>
	  /// <param name="macroMap">
	  ///          a map of macro names to regular expression strings </param>
	  /// <returns> the extended map </returns>
	  /// <exception cref="IOException">
	  ///           if there is an error when reading the configuration </exception>
	  /// <exception cref="InitializationException">
	  ///           if configuration fails </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected static java.util.Map<String, String> loadMacros(java.nio.file.Path macroPath, java.util.Map<String, String> macroMap) throws java.io.IOException
	  protected internal static IDictionary<string, string> loadMacros(Path macroPath, IDictionary<string, string> macroMap)
	  {

		// read config file
		StreamReader @in = null;
		try
		{
		  @in = new StreamReader(FileTools.openResourceFileAsStream(macroPath), Encoding.UTF8);
		}
		catch (FileNotFoundException)
		{
		  return macroMap;
		}
		string line;
		while (!string.ReferenceEquals((line = @in.ReadLine()), null))
		{
		  line = line.Trim();
		  if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
		  {
			continue;
		  }
		  int sep = line.IndexOf(":", StringComparison.Ordinal);
		  if (sep == -1)
		  {
			throw new InitializationException(string.Format("missing separator in macros configuration line {0}", line));
		  }
		  string macroName = line.Substring(0, sep).Trim();
		  string regExpString = line.Substring(sep + 1).Trim();

		  // expand possible macros
		  regExpString = replaceReferences(regExpString, macroMap);

		  macroMap[macroName] = regExpString;
		}

		return macroMap;
	  }


	  /// <summary>
	  /// Reads from the given reader until the lists section starts. Immediately returns if the reader
	  /// is {@code null}.
	  /// </summary>
	  /// <param name="in">
	  ///          the reader </param>
	  /// <exception cref="IOException">
	  ///           if there is an error when reading </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected static void readToLists(java.io.BufferedReader in) throws java.io.IOException
	  protected internal static void readToLists(StreamReader @in)
	  {

		if (null == @in)
		{
		  return;
		}

		string line;
		while (!string.ReferenceEquals((line = @in.ReadLine()), null))
		{
		  line = line.Trim();
		  if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
		  {
			continue;
		  }
		  if (line.Equals(LISTS_MARKER))
		  {
			break;
		  }
		}
	  }


	  /// <summary>
	  /// Reads from the given reader until the definitions section starts. Immediately returns if the
	  /// reader is {@code null}.
	  /// </summary>
	  /// <param name="in">
	  ///          the reader </param>
	  /// <exception cref="IOException">
	  ///           if there is an error when reading </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected static void readToDefinitions(java.io.BufferedReader in) throws java.io.IOException
	  protected internal static void readToDefinitions(StreamReader @in)
	  {

		if (null == @in)
		{
		  return;
		}

		string line;
		while (!string.ReferenceEquals((line = @in.ReadLine()), null))
		{
		  line = line.Trim();
		  if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
		  {
			continue;
		  }
		  if (line.Equals(DEFS_MARKER))
		  {
			break;
		  }
		}
	  }


	  /// <summary>
	  /// Reads the definitions section from the given reader to map each token class from the
	  /// definitions to a regular expression that matches all tokens of that class. Also extends the
	  /// given definitions map.<br>
	  /// Immediately returns if the reader is {@code null}.
	  /// </summary>
	  /// <param name="in">
	  ///          the reader </param>
	  /// <param name="macrosMap">
	  ///          a map of macro names to regular expression strings </param>
	  /// <param name="defMap">
	  ///          a map of definition names to regular expression strings </param>
	  /// <exception cref="IOException">
	  ///           if there is an error during reading </exception>
	  /// <exception cref="InitializationException">
	  ///           if configuration fails </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void loadDefinitions(java.io.BufferedReader in, java.util.Map<String, String> macrosMap, java.util.Map<String, String> defMap) throws java.io.IOException
	  protected internal virtual void loadDefinitions(StreamReader @in, IDictionary<string, string> macrosMap, IDictionary<string, string> defMap)
	  {

		if (null == @in)
		{
		  return;
		}

		// init temporary map where to store the regular expression string
		// for each class
		IDictionary<string, StringBuilder> tempMap = new Dictionary<string, StringBuilder>();

		string line;
		while (!string.ReferenceEquals((line = @in.ReadLine()), null))
		{
		  line = line.Trim();
		  if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
		  {
			continue;
		  }
		  if (line.Equals(RULES_MARKER))
		  {
			break;
		  }

		  int firstSep = line.IndexOf(":", StringComparison.Ordinal);
		  int secondSep = line.LastIndexOf(":", StringComparison.Ordinal);
		  if (firstSep == -1 || secondSep == firstSep)
		  {
			throw new InitializationException(string.Format("missing separator in definitions section line {0}", line));
		  }
		  string defName = line.Substring(0, firstSep).Trim();
		  string regExpString = line.Substring(firstSep + 1, secondSep - (firstSep + 1)).Trim();
		  string className = line.Substring(secondSep + 1).Trim();

		  // expand possible macros
		  regExpString = replaceReferences(regExpString, macrosMap);

		  // check for empty regular expression
		  if (regExpString.Length == 0)
		  {
			throw new ProcessingException(string.Format("empty regular expression in line {0}", line));
		  }

		  // extend class matcher:
		  // get old entry
		  StringBuilder oldRegExpr = tempMap[className];
		  // if there is no old entry create a new one
		  if (null == oldRegExpr)
		  {
			StringBuilder newRegExpr = new StringBuilder(regExpString);
			tempMap[className] = newRegExpr;
		  }
		  else
		  {
			// extend regular expression with another disjunct
			oldRegExpr.Append("|" + regExpString);
		  }

		  // save definition
		  defMap[defName] = regExpString;
		}

		// create regular expressions from regular expression strings and store them
		// under their class name in definitions map
		foreach (KeyValuePair<string, StringBuilder> oneEntry in tempMap.SetOfKeyValuePairs())
		{
		  try
		  {
			DefinitionsMap[oneEntry.Key] = FACTORY.createRegExp(oneEntry.Value.ToString());
		  }
		  catch (Exception e)
		  {
			throw new ProcessingException(string.Format("cannot create regular expression for {0} from {1}: {2}", oneEntry.Key, oneEntry.Value.ToString(), e.LocalizedMessage));
		  }
		}
	  }


	  /// <summary>
	  /// Reads the rules section from the given reader to map each rules to a regular expression that
	  /// matches all tokens of that rule.<br>
	  /// Immediately returns if the reader is {@code null}.
	  /// </summary>
	  /// <param name="in">
	  ///          the reader </param>
	  /// <param name="defsMap">
	  ///          a map of definition names to regular expression strings </param>
	  /// <param name="macrosMap">
	  ///          a map of macro names to regular expression strings </param>
	  /// <exception cref="IOException">
	  ///           if there is an error during reading </exception>
	  /// <exception cref="InitializationException">
	  ///           if configuration fails </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void loadRules(java.io.BufferedReader in, java.util.Map<String, String> defsMap, java.util.Map<String, String> macrosMap) throws java.io.IOException
	  protected internal virtual void loadRules(StreamReader @in, IDictionary<string, string> defsMap, IDictionary<string, string> macrosMap)
	  {

		if (null == @in)
		{
		  return;
		}

		string line;
		while (!string.ReferenceEquals((line = @in.ReadLine()), null))
		{
		  line = line.Trim();
		  if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
		  {
			continue;
		  }
		  int firstSep = line.IndexOf(":", StringComparison.Ordinal);
		  int secondSep = line.LastIndexOf(":", StringComparison.Ordinal);
		  if (firstSep == -1 || secondSep == firstSep)
		  {
			throw new InitializationException(string.Format("missing separator in rules section line {0}", line));
		  }
		  string ruleName = line.Substring(0, firstSep).Trim();
		  string regExpString = line.Substring(firstSep + 1, secondSep - (firstSep + 1)).Trim();
		  string className = line.Substring(secondSep + 1).Trim();

		  // expand definitions
		  regExpString = replaceReferences(regExpString, defsMap);
		  // expand possible macros
		  regExpString = replaceReferences(regExpString, macrosMap);

		  // add rule to map
		  RegExp regExp = FACTORY.createRegExp(regExpString);
		  RulesMap[ruleName] = regExp;
		  // if rule has a class, add regular expression to regular expression map
		  if (className.Length > 0)
		  {
			RegExpMap[regExp] = className;
		  }
		}
	  }


	  /// <summary>
	  /// Reads the lists section from the given reader to map each token class from the lists to a set
	  /// that contains all members of that class.<br>
	  /// Immediately returns if the reader is {@code null}.
	  /// </summary>
	  /// <param name="in">
	  ///          the reader </param>
	  /// <param name="resourceDir">
	  ///          the resource directory </param>
	  /// <exception cref="IOException">
	  ///           if there is an error during reading </exception>
	  /// <exception cref="InitializationException">
	  ///           if configuration fails </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void loadLists(java.io.BufferedReader in, String resourceDir) throws java.io.IOException
	  protected internal virtual void loadLists(StreamReader @in, string resourceDir)
	  {

		if (null == @in)
		{
		  return;
		}

		string line;
		while (!string.ReferenceEquals((line = @in.ReadLine()), null))
		{
		  line = line.Trim();
		  if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
		  {
			continue;
		  }
		  if (line.Equals(DEFS_MARKER))
		  {
			break;
		  }

		  int sep = line.IndexOf(":", StringComparison.Ordinal);
		  if (sep == -1)
		  {
			throw new InitializationException(string.Format("missing separator in lists section line {0}", line));
		  }
		  string listFileName = line.Substring(0, sep).Trim();
		  string className = line.Substring(sep + 1).Trim();
		  this.loadList(Paths.get(resourceDir).resolve(listFileName), className);
		}
	  }


	  /// <summary>
	  /// Loads the abbreviations list from the given path and stores its items under the given class
	  /// name
	  /// </summary>
	  /// <param name="listPath">
	  ///          the abbreviations list path </param>
	  /// <param name="className">
	  ///          the class name </param>
	  /// <exception cref="IOException">
	  ///           if there is an error when reading the list </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void loadList(java.nio.file.Path listPath, String className) throws java.io.IOException
	  private void loadList(Path listPath, string className)
	  {

		StreamReader @in = new StreamReader(FileTools.openResourceFileAsStream(listPath), Encoding.UTF8);
		// init set where to store the abbreviations
		ISet<string> items = new HashSet<string>();
		// iterate over lines of file
		string line;
		while (!string.ReferenceEquals((line = @in.ReadLine()), null))
		{
		  line = line.Trim();
		  // ignore lines starting with #
		  if (line.StartsWith("#", StringComparison.Ordinal) || (line.Length == 0))
		  {
			continue;
		  }
		  // extract the abbreviation and add it to the set
		  int end = line.IndexOf('#');
		  if (-1 != end)
		  {
			line = line.Substring(0, end).Trim();
			if (line.Length == 0)
			{
			  continue;
			}
		  }
		  items.Add(line);
		  // also add the upper case version
		  items.Add(line.ToUpper());
		  // also add a version with the first letter in
		  // upper case (if required)
		  char firstChar = line[0];
		  if (char.IsLower(firstChar))
		  {
			firstChar = char.ToUpper(firstChar);
			items.Add(firstChar + line.Substring(1));
		  }
		}
		@in.Close();
		// add set to lists map
		this.ClassMembersMap[className] = items;
	  }


	  /// <summary>
	  /// Replaces references in the given regular expression string using the given reference map.
	  /// </summary>
	  /// <param name="regExpString">
	  ///          the regular expression string with possible references </param>
	  /// <param name="refMap">
	  ///          a map of reference name to regular expression strings </param>
	  /// <returns> the modified regular expression string </returns>
	  private static string replaceReferences(string regExpString, IDictionary<string, string> refMap)
	  {

		string result = regExpString;

		IList<Match> references = REF_MATCHER.getAllMatches(regExpString);

		foreach (Match oneRef in references)
		{
		  // get reference name by removing opening and closing angle brackets
		  string refName = oneRef.Image.Substring(1, (oneRef.Image.Length - 1) - 1);
		  string refRegExpr = refMap[refName];
		  if (null == refRegExpr)
		  {
			throw new ProcessingException(string.Format("unknown reference {0} in regular expression {1}", refName, regExpString));
		  }
		  result = result.replaceFirst(oneRef.Image, string.Format("({0})", Matcher.quoteReplacement(refRegExpr)));
		}

		return result;
	  }


	  /// <summary>
	  /// Creates a rule that matches ALL definitions.
	  /// </summary>
	  /// <param name="defsMap">
	  ///          the definitions map </param>
	  /// <returns> a regular expression matching all definitions </returns>
	  protected internal static RegExp createAllRule(IDictionary<string, string> defsMap)
	  {

		StringBuilder ruleRegExpr = new StringBuilder();

		// iterate over definitions
		IList<string> defsList = new List<string>(defsMap.Values);
		for (int i = 0, iMax = defsList.Count; i < iMax; i++)
		{
		  string regExpr = defsList[i];
		  // extend regular expression with another disjunct
		  ruleRegExpr.Append(string.Format("({0})", regExpr));
		  if (i < iMax - 1)
		  {
			ruleRegExpr.Append("|");
		  }
		}
		return FACTORY.createRegExp(ruleRegExpr.ToString());
	  }
	}

}