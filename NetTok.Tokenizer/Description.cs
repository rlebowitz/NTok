using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NetTok.Tokenizer.Exceptions;

/*
 NTok
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
    ///     Abstract class that provides common methods to manage the content of description files.
    ///     @author Joerg Steffen, DFKI, Robert Lebowitz, Finaltouch IT LLC
    /// </summary>
    public abstract class Description
    {
        /// <summary>
        ///     name of the element with the definitions in the description files
        /// </summary>
        public const string Definitions = "DEFINITIONS";

        /// <summary>
        ///     attribute of a definition element that contains the regular expression
        /// </summary>
        public const string DefinitionRegularExpression = "regexp";

        /// <summary>
        ///     attribute of a definition or list element that contains the class name
        /// </summary>
        public const string DefinitionClass = "class";

        /// <summary>
        ///     name of the element with the lists in the description files
        /// </summary>
        public const string Lists = "LISTS";

        /// <summary>
        ///     attribute of a list element that point to the list file
        /// </summary>
        public const string ListFile = "file";

        /// <summary>
        ///     attribute of a list element that contains the encoding of the list file
        /// </summary>
        public const string ListEncoding = "encoding";

        /// <summary>
        ///     name of the element with the rules in the description files
        /// </summary>
        public const string Rules = "RULES";

        /// <summary>
        ///     single line in descriptions that marks the start of the lists section
        /// </summary>
        public const string ListsMarker = "LISTS:";

        /// <summary>
        ///     single line in descriptions that marks the start of the definitions section
        /// </summary>
        public const string DefinitionsMarker = "DEFINITIONS:";

        /// <summary>
        ///     single line in descriptions that marks the start of the rules section
        /// </summary>
        public const string RulesMarker = "RULES:";

        // regular expression for matching references used in regular expressions of config files
        private static readonly Regex ReferencesMatcher = new Regex("\\<[A-Za-z0-9_]+\\>");

        /// <returns> the definitions map </returns>
        public Dictionary<string, Regex> DefinitionsMap { get; set; }

        /// <returns> the rules map </returns>
        public Dictionary<string, Regex> RulesMap { get; set; }

        /// <returns> the regular expressions map </returns>
        public Dictionary<Regex, string> RegExpMap { get; set; }

        /// <returns> the class members map </returns>
        public virtual Dictionary<string, HashSet<string>> ClassMembersMap { get; set; }

        /// <summary>
        ///     Reads the macro configuration from the given path and adds it to the given map.
        /// </summary>
        /// <param name="language"></param>
        /// <param name="fileName"></param>
        /// <param name="macroMap">
        ///     a map of macro names to regular expression strings
        /// </param>
        /// <returns> the extended map </returns>
        /// <exception cref="IOException">
        ///     if there is an error when reading the configuration
        /// </exception>
        public static IDictionary<string, string> LoadMacros(string language, string fileName,
            Dictionary<string, string> macroMap)
        {
            var stream = ResourceMethods.ReadResource(language, fileName);
            var reader = new StreamReader(stream);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
                {
                    continue;
                }

                var sep = line.IndexOf(":", StringComparison.Ordinal);
                if (sep == -1)
                {
                    throw new InitializationException(
                        $"There is a missing separator in macros configuration line {line}");
                }

                var macroName = line.Substring(0, sep).Trim();
                var regExpString = line[(sep + 1)..].Trim();
                // expand possible macros
                regExpString = ReplaceReferences(regExpString, macroMap);
                macroMap[macroName] = regExpString;
            }

            return macroMap;
        }


        /// <summary>
        ///     Reads from the given reader until the lists section starts. Immediately returns if the reader
        ///     is {@code null}.
        /// </summary>
        /// <param name="reader">
        ///     the reader
        /// </param>
        /// <exception cref="IOException">
        ///     if there is an error when reading
        /// </exception>
        public static void ReadToLists(StreamReader reader)
        {
            Guard.NotNull(reader);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
                {
                    continue;
                }

                if (line.Equals(ListsMarker))
                {
                    break;
                }
            }
        }


        /// <summary>
        ///     Reads from the given reader until the definitions section starts. Immediately returns if the
        ///     reader is {@code null}.
        /// </summary>
        /// <param name="reader">
        ///     the reader
        /// </param>
        /// <exception cref="IOException">
        ///     if there is an error when reading
        /// </exception>
        public static void ReadToDefinitions(StreamReader reader)
        {
            Guard.NotNull(reader);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
                {
                    continue;
                }

                if (line.Equals(DefinitionsMarker))
                {
                    break;
                }
            }
        }


        /// <summary>
        ///     Reads the definitions section from the given reader to map each token class from the
        ///     definitions to a regular expression that matches all tokens of that class. Also extends the
        ///     given definitions map.
        ///     <br />
        ///     Immediately returns if the reader is {@code null}.
        /// </summary>
        /// <param name="reader">
        ///     the reader
        /// </param>
        /// <param name="macrosMap">
        ///     a map of macro names to regular expression strings
        /// </param>
        /// <param name="defMap">
        ///     a map of definition names to regular expression strings
        /// </param>
        /// <exception cref="IOException">
        ///     if there is an error during reading
        /// </exception>
        /// <exception cref="InitializationException">
        ///     if configuration fails
        /// </exception>
        public virtual void LoadDefinitions(StreamReader reader, IDictionary<string, string> macrosMap,
            IDictionary<string, string> defMap)
        {
            Guard.NotNull(reader);
            // init temporary map where to store the regular expression string
            // for each class
            IDictionary<string, StringBuilder> tempMap = new Dictionary<string, StringBuilder>();

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
                {
                    continue;
                }

                if (line.Equals(RulesMarker))
                {
                    break;
                }

                var firstSep = line.IndexOf(":", StringComparison.Ordinal);
                var secondSep = line.LastIndexOf(":", StringComparison.Ordinal);
                if (firstSep == -1 || secondSep == firstSep)
                {
                    throw new InitializationException($"missing separator in definitions section line {line}");
                }

                var defName = line.Substring(0, firstSep).Trim();
                var regExpString = line[(firstSep + 1)..secondSep].Trim();
                var className = line[(secondSep + 1)..].Trim();

                // expand possible macros
                regExpString = ReplaceReferences(regExpString, macrosMap);

                // check for empty regular expression
                if (regExpString.Length == 0)
                {
                    throw new ProcessingException($"empty regular expression in line {line}");
                }

                // extend class matcher:
                // get old entry
                var oldRegExpr = tempMap[className];
                // if there is no old entry create a new one
                if (null == oldRegExpr)
                {
                    var newRegExpr = new StringBuilder(regExpString);
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
            foreach (var oneEntry in tempMap)
            {
                try
                {
                    DefinitionsMap[oneEntry.Key] = new Regex(oneEntry.Value.ToString());
                }
                catch (Exception e)
                {
                    throw new ProcessingException(
                        $"Cannot create regular expression for {oneEntry.Key} from {oneEntry.Value}: {e.Message}");
                }
            }
        }


        /// <summary>
        ///     Reads the rules section from the given reader to map each rules to a regular expression that
        ///     matches all tokens of that rule.
        ///     <br />
        ///     Immediately returns if the reader is {@code null}.
        /// </summary>
        /// <param name="reader">
        ///     the reader
        /// </param>
        /// <param name="definitionsMap">
        ///     a map of definition names to regular expression strings
        /// </param>
        /// <param name="macrosMap">
        ///     a map of macro names to regular expression strings
        /// </param>
        /// <exception cref="IOException">
        ///     if there is an error during reading
        /// </exception>
        /// <exception cref="InitializationException">
        ///     if configuration fails
        /// </exception>
        public virtual void LoadRules(StreamReader reader, IDictionary<string, string> definitionsMap,
            IDictionary<string, string> macrosMap)
        {
            Guard.NotNull(reader);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
                {
                    continue;
                }

                var firstSep = line.IndexOf(":", StringComparison.Ordinal);
                var secondSep = line.LastIndexOf(":", StringComparison.Ordinal);
                if (firstSep == -1 || secondSep == firstSep)
                {
                    throw new InitializationException(
                        $"Missing separator in rules section line {line}");
                }

                var ruleName = line.Substring(0, firstSep).Trim();
                var regExpString = line[(firstSep + 1)..secondSep].Trim();
                if (secondSep + 1 >= 0 && line.Length > secondSep + 1)
                {
                    var className = line[(secondSep + 1)..].Trim();

                    // expand definitions
                    regExpString = ReplaceReferences(regExpString, definitionsMap);
                    // expand possible macros
                    regExpString = ReplaceReferences(regExpString, macrosMap);

                    // add rule to map
                    var regExp = new Regex(regExpString, RegexOptions.Compiled);
                    RulesMap[ruleName] = regExp;
                    // if rule has a class, add regular expression to regular expression map
                    if (className.Length > 0)
                    {
                        RegExpMap[regExp] = className;
                    }
                }
            }
        }


        /// <summary>
        ///     Reads the lists section from the given reader to map each token class from the lists to a set
        ///     that contains all members of that class.
        ///     <br />
        ///     Immediately returns if the reader is {@code null}.
        /// </summary>
        /// <param name="reader">
        ///     the reader
        /// </param>
        /// <exception cref="IOException">
        ///     if there is an error during reading
        /// </exception>
        /// <exception cref="InitializationException">
        ///     if configuration fails
        /// </exception>
        public void LoadLists(StreamReader reader)
        {
            Guard.NotNull(reader);

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
                {
                    continue;
                }

                if (line.Equals(DefinitionsMarker))
                {
                    break;
                }

                var separator = line.IndexOf(":", StringComparison.Ordinal);
                if (separator == -1)
                {
                    throw new InitializationException($"Missing separator in lists section line {line}");
                }

                var listFileName = line.Substring(0, separator).Trim();
                var className = line[(separator + 1)..].Trim();
                LoadList(listFileName, className);
            }
        }


        /// <summary>
        ///     Loads the abbreviations list from the given path and stores its items under the given classname
        /// </summary>
        /// <param name="listFileName">the abbreviations list path</param>
        /// <param name="className">The class name.</param>
        /// <exception cref="IOException">
        ///     if there is an error when reading the list
        /// </exception>
        private void LoadList(string listFileName, string className)
        {
            var reader = new StreamReader(ResourceMethods.ReadResource(listFileName));
            // init set where to store the abbreviations
            var items = new HashSet<string>();
            // iterate over lines of file
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();
                // ignore lines starting with #
                if (line.StartsWith("#", StringComparison.Ordinal) || line.Length == 0)
                {
                    continue;
                }

                // extract the abbreviation and add it to the set
                var end = line.IndexOf('#');
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
                var firstChar = line[0];
                if (char.IsLower(firstChar))
                {
                    firstChar = char.ToUpper(firstChar);
                    items.Add(firstChar + line.Substring(1));
                }
            }

            reader.Close();
            // add set to lists map
            ClassMembersMap[className] = items;
        }


        /// <summary>
        ///     Replaces references in the given regular expression string using the given reference map.
        /// </summary>
        /// <param name="regExpString">
        ///     the regular expression string with possible references
        /// </param>
        /// <param name="refMap">A map of reference name to regular expression strings</param>
        /// <returns>The modified regular expression string.</returns>
        private static string ReplaceReferences(string regExpString, IDictionary<string, string> refMap)
        {
            var result = regExpString;

            //   var references = _referencesMatcher.GetAllMatches(regExpString);
            var matches = ReferencesMatcher.Matches(regExpString);
            var references = matches.Select(match => match).ToList();

            foreach (var reference in references)
            {
                // get reference name by removing opening and closing angle brackets
                var refName = reference.Value[1..^1];
                var refRegExpr = refMap[refName];
                if (null == refRegExpr)
                {
                    throw new ProcessingException($"unknown reference {refName} in regular expression {regExpString}");
                }

                //result = result.replaceFirst(reference.Image,
                //    string.Format("({0})", Matcher.quoteReplacement(refRegExpr)));
                // ToDo - not sure exactly what the above lines do, this is my best guess:
                var regex = new Regex(reference.Value);
                result = regex.Replace(result, $"({refRegExpr})", 1);
            }

            return result;
        }


        /// <summary>
        ///     Creates a rule that matches ALL definitions.
        /// </summary>
        /// <param name="definitionsMap">The definitions map.</param>
        /// <returns> a regular expression matching all definitions </returns>
        public Regex CreateAllRule(IDictionary<string, string> definitionsMap)
        {
            var ruleRegExpr = new StringBuilder();

            // iterate over definitions
            IList<string> definitionsList = new List<string>(definitionsMap.Values);
            for (int i = 0, iMax = definitionsList.Count; i < iMax; i++)
            {
                var regularExpression = definitionsList[i];
                // extend regular expression with another disjunct
                ruleRegExpr.Append($"({regularExpression})");
                if (i < iMax - 1)
                {
                    ruleRegExpr.Append("|");
                }
            }

            return new Regex(ruleRegExpr.ToString());
        }
    }
}