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
	/// Manages the content of a token classes description file.
	/// 
	/// @author Joerg Steffen, DFKI
	/// </summary>
	public class TokenClassesDescription : Description
	{

	  /// <summary>
	  /// name of the all classes rule </summary>
	  protected internal const string ALL_RULE = "ALL_CLASSES_RULE";


	  // name suffix of the resource file with the token classes description
	  private const string CLASS_DESCR = "_classes.cfg";


	  /// <summary>
	  /// Creates a new instance of <seealso cref="TokenClassesDescription"/> for the given language.
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
//ORIGINAL LINE: public TokenClassesDescription(String resourceDir, String lang, java.util.Map<String, String> macrosMap) throws java.io.IOException
	  public TokenClassesDescription(string resourceDir, string lang, IDictionary<string, string> macrosMap)
	  {

		base.DefinitionsMap = new Dictionary<string, RegExp>();
		base.RulesMap = new Dictionary<string, RegExp>();
		base.RegExpMap = new Dictionary<RegExp, string>();

		Path tokClassesDescrPath = Paths.get(resourceDir).resolve(lang + CLASS_DESCR);
		StreamReader @in = new StreamReader(FileTools.openResourceFileAsStream(tokClassesDescrPath), Encoding.UTF8);

		// read config file to definitions start
		ReadToDefinitions(@in);

		// read definitions
		IDictionary<string, string> defsMap = new Dictionary<string, string>();
		base.LoadDefinitions(@in, macrosMap, defsMap);

		RulesMap[ALL_RULE] = createAllRule(defsMap);

		@in.Close();
	  }
	}

}