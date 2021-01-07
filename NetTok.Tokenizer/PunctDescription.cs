using System.Collections.Generic;
using System.IO;
using System.Text;
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

	using RegExp = RegExp;

	/// <summary>
	/// Manages the content of a punctuation description file.
	/// 
	/// @author Joerg Steffen, DFKI
	/// </summary>
	public class PunctDescription : Description
	{

	  /// <summary>
	  /// class name for opening punctuation </summary>
	  public const string OPEN_PUNCT = "OPEN_PUNCT";

	  /// <summary>
	  /// class name for closing punctuation </summary>
	  public const string CLOSE_PUNCT = "CLOSE_PUNCT";

	  /// <summary>
	  /// class name for opening brackets </summary>
	  public const string OPEN_BRACKET = "OPEN_BRACKET";

	  /// <summary>
	  /// class name for closing brackets </summary>
	  public const string CLOSE_BRACKET = "CLOSE_BRACKET";

	  /// <summary>
	  /// class name for terminal punctuation </summary>
	  public const string TERM_PUNCT = "TERM_PUNCT";

	  /// <summary>
	  /// class name for possible terminal punctuation </summary>
	  public const string TERM_PUNCT_P = "TERM_PUNCT_P";


	  /// <summary>
	  /// name of the all punctuation rule </summary>
	  protected internal const string ALL_RULE = "ALL_PUNCT_RULE";

	  /// <summary>
	  /// name of the internal punctuation rule </summary>
	  protected internal const string INTERNAL_RULE = "INTERNAL_PUNCT_RULE";

	  /// <summary>
	  /// name of the sentence internal punctuation rule </summary>
	  protected internal const string INTERNAL_TU_RULE = "INTERNAL_TU_PUNCT_RULE";

	  /// <summary>
	  /// class name for ambiguous open/close punctuation </summary>
	  protected internal const string OPEN_CLOSE_PUNCT = "OPENCLOSE_PUNCT";

	  // name suffix of the resource file with the punctuation description
	  private const string PUNCT_DESCR = "_punct.cfg";


	  /// <summary>
	  /// Creates a new instance of <seealso cref="PunctDescription"/> for the given language.
	  /// </summary>
	  /// <param name="resourceDir">
	  ///          path to the folder with the language resources </param>
	  /// <param name="lang">
	  ///          the language </param>
	  /// <param name="macrosMap">
	  ///          a map of macro names to regular expression strings </param>
	  /// <exception cref="IOException">
	  ///           if there is an error when reading the configuration </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public PunctDescription(String resourceDir, String lang, java.util.Map<String, String> macrosMap) throws java.io.IOException
	  public PunctDescription(string resourceDir, string lang, IDictionary<string, string> macrosMap)
	  {

		base.DefinitionsMap = new Dictionary<string, RegExp>();
		base.RulesMap = new Dictionary<string, RegExp>();
		base.RegExpMap = new Dictionary<RegExp, string>();

		Path punctDescrPath = Paths.get(resourceDir).resolve(lang + PUNCT_DESCR);
		StreamReader @in = new StreamReader(FileTools.openResourceFileAsStream(punctDescrPath), Encoding.UTF8);

		// read config file to definitions start
		ReadToDefinitions(@in);

		// read definitions
		IDictionary<string, string> defsMap = new Dictionary<string, string>();
		base.LoadDefinitions(@in, macrosMap, defsMap);

		// when loadDefinitions returns the reader has reached the rules section;
		// read rules
		base.loadRules(@in, defsMap, macrosMap);

		RulesMap[ALL_RULE] = createAllRule(defsMap);

		@in.Close();
	  }
	}

}