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

	using Logger = org.slf4j.Logger;
    using RegExp = RegExp;

	/// <summary>
	/// Provides methods to collect abbreviations from corpora containing a single sentence per line.
	/// 
	/// @author Joerg Steffen, DFKI
	/// </summary>
	public sealed class AbbrevCollector
	{

	  // the logger
	  private static readonly Logger logger = LoggerFactory.getLogger(typeof(AbbrevCollector));


	  // would create a new instance of {@link AbbrevCollector}; not to be used
	  private AbbrevCollector()
	  {

		// private constructor to enforce noninstantiability
	  }


	  /// <summary>
	  /// Scans the given directory recursively for files with the given suffix. It is assumed that each
	  /// of these files contains one sentence per line. It extracts all abbreviations from these files
	  /// and stores them under the given result file name using UTF-8 encoding.
	  /// </summary>
	  /// <param name="dir">
	  ///          the directory to scan </param>
	  /// <param name="suffix">
	  ///          the file name suffix </param>
	  /// <param name="encoding">
	  ///          the encoding of the files </param>
	  /// <param name="resultFileName">
	  ///          the result file name </param>
	  /// <param name="lang">
	  ///          the language of the files </param>
	  /// <exception cref="IOException">
	  ///           if there is a problem when reading or writing the files </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void collect(String dir, String suffix, String encoding, String resultFileName, String lang) throws java.io.IOException
	  public static void collect(string dir, string suffix, string encoding, string resultFileName, string lang)
	  {

		// init tokenizer and get the relevant language resource
		NTok jtok = new NTok();
		LanguageResource langRes = jtok.getLanguageResource(lang);

		// get matchers and lists used to filter the abbreviations

		// the lists contains known abbreviations and titles
		IDictionary<string, ISet<string>> abbrevLists = langRes.AbbreviationLists;

		// this contains the word that only start with a capital letter at
		// the beginning of a sentence; we want to avoid to extract abbreviations
		// consisting of such a word followed by a punctuation
		ISet<string> nonCapTerms = langRes.NonCapitalizedTerms;

		// this are the matcher for abbreviations
		RegExp abbrevMatcher = langRes.AllAbbreviationMatcher;

		ISet<string> abbrevs = new HashSet<string>();

		// get all training files
		IList<string> trainingFiles = FileTools.getFilesFromDir(dir, suffix);

		// iterate over corpus files
		foreach (string oneFileName in trainingFiles)
		{
		  logger.info("processing " + oneFileName + " ...");

		  // init reader
		  StreamReader @in = new StreamReader(new FileStream(oneFileName, FileMode.Open, FileAccess.Read), encoding);
		  string sent;
		  // read lines from file
		  while (!string.ReferenceEquals((sent = @in.ReadLine()), null))
		  {

			// split the sentence using as separator whitespaces and
			// ... .. ' ` \ \\ |
			string[] tokens = sent.Split(" |\\.\\.\\.|\\.\\.|'|`|\\(|\\)|[|]", true);

			for (int i = 0; i < (tokens.Length - 1); i++)
			{
			  // we skip the last token with the final sentence punctuation
			  string oneTok = tokens[i];
			  if ((oneTok.Length > 1) && oneTok.EndsWith(".", StringComparison.Ordinal))
			  {

				// if the abbreviation contains a hyphen, it's sufficient to check
				// the part after the hyphen
				int hyphenPos = oneTok.LastIndexOf("-", StringComparison.Ordinal);
				if (hyphenPos != -1)
				{
				  oneTok = oneTok.Substring(hyphenPos + 1);
				}

				// check with matchers
				if (abbrevMatcher.matches(oneTok))
				{
				  continue;
				}

				// check with lists
				bool found = false;
				foreach (KeyValuePair<string, ISet<string>> oneEntry in abbrevLists.SetOfKeyValuePairs())
				{
				  ISet<string> oneList = oneEntry.Value;
				  if (oneList.Contains(oneTok))
				  {
					found = true;
					break;
				  }
				}
				if (found)
				{
				  continue;
				}

				// check with terms;
				// convert first letter to upper case because this is the format of
				// the terms in the list and remove the punctuation
				char firstChar = oneTok[0];
				firstChar = char.ToUpper(firstChar);
				string tempTok = firstChar + oneTok.Substring(1, (oneTok.Length - 1) - 1);
				if (nonCapTerms.Contains(tempTok))
				{
				  continue;
				}

				// we found a new abbreviation
				abbrevs.Add(oneTok);
			  }
			}
		  }
		  @in.Close();
		}

		// sort collected abbreviations
		IList<string> sortedAbbrevs = new List<string>(abbrevs);
		sortedAbbrevs.Sort();

		// save results
		PrintWriter @out = null;
		try
		{
		  @out = new PrintWriter(new StreamWriter(new FileStream(resultFileName, FileMode.Create, FileAccess.Write), Encoding.UTF8));
		  foreach (string oneAbbrev in sortedAbbrevs)
		  {
			@out.println(oneAbbrev);
		  }
		}
		catch (IOException e)
		{
		  Console.WriteLine(e.ToString());
		  Console.Write(e.StackTrace);
		}
		finally
		{
		  if (null != @out)
		  {
			@out.close();
		  }
		}
	  }


	  /// <summary>
	  /// This is the main method. It requires 5 arguments:
	  /// <ul>
	  /// <li>the parent folder of the corpus
	  /// <li>the file extension of the corpus files to use
	  /// <li>the file encoding
	  /// <li>the result file name
	  /// <li>the language of the corpus
	  /// </ul>
	  /// </summary>
	  /// <param name="args">
	  ///          an array with the arguments </param>
	  public static void Main(string[] args)
	  {

		if (args.Length != 5)
		{
		  Console.Error.WriteLine("wrong number of arguments");
		  Environment.Exit(1);
		}

		try
		{
		  AbbrevCollector.collect(args[0], args[1], args[2], args[3], args[4]);
		}
		catch (IOException e)
		{
		  Console.WriteLine(e.ToString());
		  Console.Write(e.StackTrace);
		}
	  }
	}

}