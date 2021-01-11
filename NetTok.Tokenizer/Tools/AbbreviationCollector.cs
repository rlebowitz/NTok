using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

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

namespace NetTok.Tokenizer.Tools
{
    /// <summary>
    ///     Provides methods to collect abbreviations from corpora containing a single sentence per line.
    ///     @author Joerg Steffen, DFKI
    /// </summary>
    public static class AbbreviationCollector
    {
        // the logger
        private static readonly ILoggerFactory LoggerFactory = new LoggerFactory();
        private static readonly Logger<NTok> Logger = new Logger<NTok>(LoggerFactory);

        /// <summary>
        ///     Scans the given directory recursively for files with the given suffix. It is assumed that each
        ///     of these files contains one sentence per line. It extracts all abbreviations from these files
        ///     and stores them under the given result file name using UTF-8 encoding.
        /// </summary>
        /// <param name="dir">
        ///     the directory to scan
        /// </param>
        /// <param name="suffix">
        ///     the file name suffix
        /// </param>
        /// <param name="encoding">
        ///     the encoding of the files
        /// </param>
        /// <param name="resultFileName">
        ///     the result file name
        /// </param>
        /// <param name="language">
        ///     the language of the files
        /// </param>
        /// <exception cref="IOException">
        ///     if there is a problem when reading or writing the files
        /// </exception>
        public static void Collect(string dir, string suffix, string encoding, string resultFileName, string language)
        {
            // init tokenizer and get the relevant language resource
            var nTok = new NTok();
            var resource = nTok.GetLanguageResource(language);

            // get matchers and lists used to filter the abbreviations

            // the lists contains known abbreviations and titles
            var abbreviationLists = resource.AbbreviationLists;

            // this contains the word that only start with a capital letter at
            // the beginning of a sentence; we want to avoid to extract abbreviations
            // consisting of such a word followed by a punctuation
            ISet<string> nonCapTerms = resource.NonCapitalizedTerms;

            // this are the matcher for abbreviations
            var abbrevMatcher = resource.AllAbbreviationMatcher;

            ISet<string> abbreviations = new HashSet<string>();

            // get all training files
            var trainingFiles = Directory.GetFiles(dir, "*suffix");
            // iterate over corpus files
            foreach (var oneFileName in trainingFiles)
            {
                if (File.Exists(oneFileName))
                {
                    Logger.LogInformation($"processing {oneFileName} ...");
                }
                else
                {
                    Logger.LogError($"Training File: {oneFileName} does not exist.");
                    continue;
                }

                // init reader
                var reader = new StreamReader(File.OpenRead(oneFileName));
                string sent;
                // read lines from file
                var regex = new Regex(" |\\.\\.\\.|\\.\\.|'|`|\\(|\\)|[|]");
                while ((sent = reader.ReadLine()) != null)
                {
                    // split the sentence using as separator whitespaces and
                    // ... .. ' ` \ \\ |
                    var tokens = regex.Split(sent);

                    for (var i = 0; i < tokens.Length - 1; i++)
                    {
                        // we skip the last token with the final sentence punctuation
                        var oneTok = tokens[i];
                        if (oneTok.Length > 1 && oneTok.EndsWith(".", StringComparison.Ordinal))
                        {
                            // if the abbreviation contains a hyphen, it's sufficient to check
                            // the part after the hyphen
                            var hyphenPos = oneTok.LastIndexOf("-", StringComparison.Ordinal);
                            if (hyphenPos != -1)
                            {
                                oneTok = oneTok.Substring(hyphenPos + 1);
                            }

                            // check with matchers
                            if (abbrevMatcher.Matches(oneTok).Any())
                            {
                                continue;
                            }

                            // check with lists
                            var found = abbreviationLists.Select(oneEntry => oneEntry.Value)
                                .Any(oneList => ((ISet<string>) oneList).Contains(oneTok));
                            if (found)
                            {
                                continue;
                            }

                            // check with terms;
                            // convert first letter to upper case because this is the format of
                            // the terms in the list and remove the punctuation
                            var firstChar = oneTok[0];
                            firstChar = char.ToUpper(firstChar);
                            var tempTok = firstChar + oneTok[1..^1];
                            if (nonCapTerms.Contains(tempTok))
                            {
                                continue;
                            }

                            // we found a new abbreviation
                            abbreviations.Add(oneTok);
                        }
                    }
                }

                reader.Close();
            }

            // sort collected abbreviations
            var sortedAbbrevs = new List<string>(abbreviations);
            sortedAbbrevs.Sort();

            // save results
            using var stream = File.Create(resultFileName);
            using var writer = new StreamWriter(stream);
            try
            {
                foreach (var oneAbbrev in sortedAbbrevs)
                {
                    writer.WriteLine(oneAbbrev);
                }
            }
            catch (IOException e)
            {
                Logger.LogError("Error while writing out abbreviations.", e);
            }
        }


        /// <summary>
        ///     This is the main method. It requires 5 arguments:
        ///     <ul>
        ///         <li>the parent folder of the corpus</li>
        ///         <li>the file extension of the corpus files to use</li>
        ///         <li>the file encoding</li>
        ///         <li>the result file name</li>
        ///         <li>the language of the corpus</li>
        ///     </ul>
        /// </summary>
        /// <param name="args">An array with the arguments.</param>
        public static void Main(string[] args)
        {
            if (args.Length != 5)
            {
                Console.Error.WriteLine("wrong number of arguments");
                Environment.Exit(1);
            }

            try
            {
                Collect(args[0], args[1], args[2], args[3], args[4]);
            }
            catch (IOException e)
            {
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }
        }
    }
}