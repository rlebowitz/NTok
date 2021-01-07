using System;
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
	/// Manages the content of a abbreviation description file.
	/// 
	/// @author Joerg Steffen, DFKI
	/// </summary>
	public class AbbrevDescription : Description
	{

	  /// <summary>
	  /// class name for breaking abbreviation </summary>
	  public const string B_ABBREVIATION = "B_ABBREVIATION";

	  /// <summary>
	  /// name of the all abbreviation rule </summary>
	  protected internal const string ALL_RULE = "ALL_RULE";

	  // name suffix of the resource file with the abbreviations description
	  private const string ABBREV_DESCR = "_abbrev.cfg";


	  // the most common terms that only start with a capital letter when they are at the beginning
	  // of a sentence
	  private ISet<string> nonCapTerms;


	  /// <summary>
	  /// Creates a new instance of <seealso cref="AbbrevDescription"/> for the given language.
	  /// </summary>
	  /// <param name="resourceDir">
	  ///          path to the folder with the language resources </param>
	  /// <param name="lang">
	  ///          the language </param>
	  /// <param name="macrosMap">
	  ///          a map of macro names to regular expression strings </param>
	  /// <exception cref="IOException">
	  ///           if there is an error when reading the configuration </exception>
	  public AbbrevDescription(string lang, IDictionary<string, string> macrosMap)
	  {

		base.DefinitionsMap = new Dictionary<string, RegExp>();
		base.RulesMap = new Dictionary<string, RegExp>();
		base.RegExpMap = new Dictionary<RegExp, string>();
		base.ClassMembersMap = new Dictionary<string, HashSet<string>>();

		Path abbrDescrPath = Paths.get(resourceDir).resolve(lang + ABBREV_DESCR);
		StreamReader @in = new StreamReader(FileTools.openResourceFileAsStream(abbrDescrPath), Encoding.UTF8);

		// read config file to lists start
		readToLists(@in);

		// read lists
		base.LoadLists(@in, resourceDir);

		// read definitions
		IDictionary<string, string> defsMap = new Dictionary<string, string>();
		base.LoadDefinitions(@in, macrosMap, defsMap);

		RulesMap[ALL_RULE] = createAllRule(defsMap);

		@in.Close();

		// load list of terms that only start with a capital letter when they are
		// at the beginning of a sentence
		Path nonCapTermsPath = Paths.get(resourceDir).resolve(lang + "_nonCapTerms.txt");
		StreamReader nonCopTermsIn = new StreamReader(FileTools.openResourceFileAsStream(nonCapTermsPath), Encoding.UTF8);

		readNonCapTerms(nonCopTermsIn);

		nonCopTermsIn.Close();
	  }


	  /// <summary>
	  /// Returns the set of the most common terms that only start with a capital letter when they are at
	  /// the beginning of a sentence.
	  /// </summary>
	  /// <returns> a set with the terms </returns>
	  protected internal virtual ISet<string> NonCapTerms
	  {
		  get
		  {
    
			return this.nonCapTerms;
		  }
	  }


	  /// <summary>
	  /// Reads the list of terms that only start with a capital letter when they are at the beginning of
	  /// a sentence from the given reader.<br>
	  /// Immediately returns if the reader is {@code null}.
	  /// </summary>
	  /// <param name="in">
	  ///          the reader </param>
	  /// <exception cref="IOException">
	  ///           if there is an error when reading </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void readNonCapTerms(java.io.BufferedReader in) throws java.io.IOException
	  private void readNonCapTerms(StreamReader @in)
	  {

		if (null == @in)
		{
		  return;
		}

		// init set where to store the terms
		this.nonCapTerms = new HashSet<string>();

		string line;
		while (!string.ReferenceEquals((line = @in.ReadLine()), null))
		{
		  line = line.Trim();
		  // ignore lines starting with #
		  if (line.StartsWith("#", StringComparison.Ordinal) || (line.Length == 0))
		  {
			continue;
		  }
		  // extract the term and add it to the set
		  int end = line.IndexOf('#');
		  if (-1 != end)
		  {
			line = line.Substring(0, end).Trim();
			if (line.Length == 0)
			{
			  continue;
			}
		  }

		  // convert first letter to upper case to make runtime comparison more
		  // efficient
		  char firstChar = line[0];
		  firstChar = char.ToUpper(firstChar);
		  this.nonCapTerms.Add(firstChar + line.Substring(1));
		  // also add a version completely in upper case letters
		  this.nonCapTerms.Add(line.ToUpper());
		}
	  }
	}

}