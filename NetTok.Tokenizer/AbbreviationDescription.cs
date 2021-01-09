using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

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

namespace NetTok.Tokenizer
{
    /// <summary>
    ///     Manages the content of a abbreviation description file.
    ///     @author Joerg Steffen, DFKI, Robert J Lebowitz, Finaltouch IT LLC
    /// </summary>
    public class AbbreviationDescription : Description
    {
        /// <summary>
        ///     Class name for breaking abbreviation
        /// </summary>
        public const string BAbbreviation = "B_ABBREVIATION";

        /// <summary>
        ///     name of the all abbreviation rule
        /// </summary>
        protected internal const string AllRule = "ALL_RULE";

        /// <summary>
        ///     name suffix of the resource file with the abbreviations description
        /// </summary>
        private const string AbbreviationDescriptionSuffix = "_abbrev.cfg";


        // the most common terms that only start with a capital letter when they are at the beginning
        // of a sentence


        /// <summary>
        ///     Creates a new instance of <seealso cref="AbbreviationDescription" /> for the given language.
        /// </summary>
        /// <param name="language">The specified language.</param>
        /// <param name="macrosMap">A map of macro names to regular expression strings.</param>
        /// <exception cref="IOException">If there is an error when reading the configuration.</exception>
        public AbbreviationDescription(string language, IDictionary<string, string> macrosMap)
        {
            DefinitionsMap = new Dictionary<string, Regex>();
            RulesMap = new Dictionary<string, Regex>();
            RegExpMap = new Dictionary<Regex, string>();
            base.ClassMembersMap = new Dictionary<string, HashSet<string>>();

            using (var reader = new StreamReader(ResourceMethods.ReadResource(language, AbbreviationDescriptionSuffix)))
            {
                // read config file to lists start
                ReadToLists(reader);
                // read lists
                LoadLists(reader);
                // read definitions
                IDictionary<string, string> definitionsMap = new Dictionary<string, string>();
                base.LoadDefinitions(reader, macrosMap, definitionsMap);
                RulesMap[AllRule] = CreateAllRule(definitionsMap);
            }

            // load list of terms that only start with a capital letter when they are
            // at the beginning of a sentence
            using (var reader = new StreamReader(ResourceMethods.ReadResource($"{language}_nonCapTerms.txt")))
            {
                ReadNonCapTerms(reader);
            }
        }

        /// <summary>
        ///     Returns the set of the most common terms that only start with a capital letter when they are at
        ///     the beginning of a sentence.
        /// </summary>
        /// <returns> a set with the terms </returns>
        public virtual HashSet<string> NonCapTerms { get; private set; }

        /// <summary>
        ///     Reads the list of terms that only start with a capital letter when they are at the beginning of
        ///     a sentence from the given reader.<br />
        ///     Immediately returns if the reader is null.
        /// </summary>
        /// <param name="reader">The specified StreamReader.</param>
        /// <exception cref="IOException">Thrown if there is an error when reading.</exception>
        private void ReadNonCapTerms(TextReader reader)
        {
            if (null == reader)
            {
                return;
            }

            // init set where to store the terms
            NonCapTerms = new HashSet<string>();

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();
                // ignore lines starting with #
                if (line.StartsWith("#", StringComparison.Ordinal) || line.Length == 0)
                {
                    continue;
                }

                // extract the term and add it to the set
                var end = line.IndexOf('#');
                if (-1 != end)
                {
                    line = line.Substring(0, end).Trim();
                    if (line.Length == 0)
                    {
                        continue;
                    }
                }

                // convert first letter to upper case to make runtime comparison more efficient
                var firstChar = line[0];
                firstChar = char.ToUpper(firstChar);
                NonCapTerms.Add($"{firstChar}{line[1..]}");
                // also add a version completely in upper case letters
                NonCapTerms.Add(line.ToUpper());
            }
        }
    }
}