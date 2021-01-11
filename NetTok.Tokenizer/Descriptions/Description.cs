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

namespace NetTok.Tokenizer.Descriptions
{
    /// <summary>
    ///     Abstract class that provides common methods to manage the content of description files.
    ///     @author Joerg Steffen, DFKI, Robert Lebowitz, Finaltouch IT LLC
    /// </summary>
    public abstract class Description
    {
        // regular expression for matching references used in regular expressions of config files
        private static readonly Regex ReferencesRegex = new Regex("\\<[A-Za-z0-9_]+\\>");

        /// <returns> the definitions map </returns>
        public Dictionary<string, Regex> DefinitionsMap { get; set; }

        /// <returns> the rules map </returns>
        public Dictionary<string, Regex> RulesMap { get; set; }

        /// <returns> the regular expressions map </returns>
        public Dictionary<Regex, string> RegExpMap { get; set; }

        /// <returns> the class members map </returns>
        public Dictionary<string, HashSet<string>> ClassMembersMap { get; set; }

        protected static char[] Delimiters { get; } = new[] {':', '\t'};

        /// <summary>
        ///     Reads the embedded macro configuration file for the specified language and adds its data to the specified map.
        /// </summary>
        /// <param name="language">The specified language.</param>
        /// <param name="fileName">The specified embedded resource file.</param>
        /// <param name="macroMap">A map of macro names to regular expression pattern strings.</param>
        /// <returns>The populated class map.</returns>
        /// <exception cref="IOException">
        ///     if there is an error when reading the configuration
        /// </exception>
        public static IDictionary<string, string> LoadMacros(string language, string fileName,
            Dictionary<string, string> macroMap)
        {
            Guard.NotNull(fileName);
            Guard.NotNull(macroMap);
            using var reader = new StreamReader(ResourceManager.Read(language, fileName));
            string line;
            while ((line = reader.ReadLine()?.Trim()) != null)
            {
                if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
                {
                    continue;
                }

                var sections = line.Split(Delimiters, StringSplitOptions.RemoveEmptyEntries);
                if (sections.Length != 2)
                {
                    throw new InitializationException($"File: {fileName} Line: {line} is not properly formatted as a Macros line.");
                }

                var macroName = sections[0].Trim();
                var regularExpressionString = sections[1].Trim();
                // expand possible macros
                regularExpressionString = ReplaceReferences(regularExpressionString, macroMap);
                macroMap[macroName] = regularExpressionString;
            }

            return macroMap;
        }

        /// <summary>
        ///     Reads from the given reader to the start of the LISTS: section or if the reader returns null.
        /// </summary>
        /// <param name="reader">The specified reader</param>
        /// <exception cref="IOException">Thrown if an error occurs while reading the embedded resource file.</exception>
        public static void ReadToLists(StreamReader reader)
        {
            Guard.NotNull(reader);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
                {
                    continue; // skip any blank lines or comment lines.
                }

                if (line.Equals(Constants.Descriptions.ListsMarker))
                {
                    break;
                }
            }
        }


        /// <summary>
        ///     Reads from the given reader to the start of the DEFINITIONS: section or the reader returns null.
        /// </summary>
        /// <param name="reader">The specified StreamReader.</param>
        /// <exception cref="IOException">Thrown if there is an error occurs while reading when reading the configuration file.</exception>
        public static void ReadToDefinitions(StreamReader reader)
        {
            Guard.NotNull(reader);
            string line;
            while ((line = reader.ReadLine()?.Trim()) != null)
            {
                if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
                {
                    continue; // skip any blank lines or comment lines.
                }

                if (line.Equals(Constants.Descriptions.DefinitionsMarker))
                {
                    break;
                }
            }
        }

        /// <summary>
        ///     Reads the DEFINITIONS: section from the given reader to map each token class from the
        ///     definitions to a regular expression that matches all tokens of that class.
        /// </summary>
        /// <param name="reader">The specified StreamReader</param>
        /// <param name="macrosMap">The specified macro map of names to regular expression pattern strings.</param>
        /// <param name="definitionMap">The specified map of macro definition names to regular expression pattern strings.</param>
        /// <exception cref="IOException">
        ///     if there is an error during reading
        /// </exception>
        public void LoadDefinitions(StreamReader reader, IDictionary<string, string> macrosMap,
            IDictionary<string, string> definitionMap)
        {
            Guard.NotNull(reader);
            Guard.NotNull(macrosMap);
            Guard.NotNull(definitionMap);
            // init temporary map where to store the regular expression string for each class
            IDictionary<string, StringBuilder> map = new Dictionary<string, StringBuilder>();

            string line;
            while ((line = reader.ReadLine()?.Trim()) != null)
            {
                if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
                {
                    continue; // skip blank lines and comments
                }

                if (line.Equals(Constants.Descriptions.RulesMarker))
                {
                    break;
                }

                var sections = line.Split(Delimiters, StringSplitOptions.RemoveEmptyEntries);
                
                if (!line.StartsWith("COLON") && sections.Length != 3)
                {
                    throw new InitializationException($"Line: {line} is not properly formatted as a Definitions line.");
                }

                if (line.StartsWith("COLON"))
                {
                    // special case
                    sections = new [] {"COLON", ":", "COLON"};
                }

                var definitionName = sections[0].Trim();
                var regularExpressionPattern = sections[1].Trim();
                var className = sections[2].Trim();
                // expand possible macros
                regularExpressionPattern = ReplaceReferences(regularExpressionPattern, macrosMap);
                // check for empty regular expression
                if (regularExpressionPattern.Length == 0)
                {
                    throw new ProcessingException($"empty regular expression in line {line}");
                }

                switch (map.ContainsKey(className))
                {
                    case false:
                    {
                        // if there is no old entry, create a new one
                        var newRegExpr = new StringBuilder(regularExpressionPattern);
                        map[className] = newRegExpr;
                        break;
                    }
                    default:
                        // extend regular expression pattern with another disjunct
                        map[className].Append('|').Append(regularExpressionPattern);
                        break;
                }

                // save definition
                definitionMap[definitionName] = regularExpressionPattern;
            }

            // create regular expressions from regular expression pattern strings and store them
            // in the definitions map
            foreach (var (key, value) in map)
            {
                DefinitionsMap[key] = new Regex(value.ToString());
            }
        }

        /// <summary>
        ///     Reads the RULES: section from the given embedded configuration file to map each rule to a regular expression
        ///     pattern
        ///     that matches all tokens of that rule.
        /// </summary>
        /// <param name="reader">The specified StreamReader</param>
        /// <param name="definitionsMap">The specified map of definition names to regular expression pattern strings</param>
        /// <param name="macrosMap">The specified map of macro names to regular expression pattern strings.</param>
        /// <exception cref="IOException">Thrown if there an error occurs while reading the embedded configuration file.</exception>
        public virtual void LoadRules(StreamReader reader, IDictionary<string, string> definitionsMap,
            IDictionary<string, string> macrosMap)
        {
            Guard.NotNull(reader);
            Guard.NotNull(definitionsMap);
            Guard.NotNull(macrosMap);
            string line;
            while ((line = reader.ReadLine()?.Trim()) != null)
            {
                if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
                {
                    continue; // skip any blank or comment lines.
                }

                var sections = line.Split(Delimiters, StringSplitOptions.RemoveEmptyEntries);
                if (sections.Length < 2)
                {
                    throw new InitializationException($"Line: {line} is not properly formatted as a Rules line.");
                }

                var ruleName = sections[0].Trim();
                var regularExpressionPattern = sections[1].Trim();
                // expand definitions
                regularExpressionPattern = ReplaceReferences(regularExpressionPattern, definitionsMap);
                // expand possible macros
                regularExpressionPattern = ReplaceReferences(regularExpressionPattern, macrosMap);
                // add rule to map
                var regularExpression = new Regex(regularExpressionPattern, RegexOptions.Compiled);
                RulesMap[ruleName] = regularExpression;
                // if rule has a class, add regular expression to regular expression map
                if (sections.Length == 3 && sections[2].Trim().Length > 0)
                {
                    RegExpMap[regularExpression] = sections[2].Trim();
                }
            }
        }

        /// <summary>
        ///     Reads the lists section from the given reader to map each token class from the lists to a set
        ///     that contains all members of that class.
        /// </summary>
        /// <param name="reader">The specified StreamReader.</param>
        /// <exception cref="IOException">Thrown if an error occurs while reading the embedded configuration file.</exception>
        /// <exception cref="InitializationException">Thrown if the configuration fails.</exception>
        public void LoadLists(StreamReader reader)
        {
            Guard.NotNull(reader);
            string line;
            while ((line = reader.ReadLine()?.Trim()) != null)
            {
                if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
                {
                    continue; // skip blank and comment lines.
                }

                if (line.Equals(Constants.Descriptions.DefinitionsMarker))
                {
                    break;
                }

                var sections = line.Split(Delimiters, StringSplitOptions.RemoveEmptyEntries);
                if (sections.Length != 2)
                {
                    throw new InitializationException($"Line: {line} is not properly formatted as a Lists line.");
                }

                var abbreviationsListFileName = sections[0].Trim();
                var className = sections[1].Trim();
                LoadList(abbreviationsListFileName, className);
            }
        }

        /// <summary>
        ///     Loads the abbreviations list from the given path and stores its items under the given classname.
        /// </summary>
        /// <param name="abbreviationsListFileName">The specified embedded abbreviations list file name.</param>
        /// <param name="className">The specified class name.</param>
        /// <exception cref="IOException">Thrown if an error occurs when reading the list file.</exception>
        private void LoadList(string abbreviationsListFileName, string className)
        {
            Guard.NotNull(abbreviationsListFileName);
            Guard.NotNull(className);
            using var reader = new StreamReader(ResourceManager.Read(abbreviationsListFileName));
            // initialize the HashSet in which the abbreviations will be stored.
            var abbreviations = new HashSet<string>();
            // iterate over lines of file
            string line;
            while ((line = reader.ReadLine()?.Trim()) != null)
            {
                if (line.StartsWith("#", StringComparison.Ordinal) || line.Length == 0)
                {
                    continue; // skip blank and comment lines.
                }

                var sections = line.Split(new char[]{'#', '\t'}, StringSplitOptions.RemoveEmptyEntries);
                if (sections.Length == 0)
                {
                    continue;
                }

                abbreviations.Add(sections[0]);
                // also add the upper case version
                abbreviations.Add(sections[0].ToUpper());
                // also add a version with the first letter in upper case
                abbreviations.Add($"{char.ToUpper(sections[0][0])}{sections[0][1..]}");
            }

            // add set to lists map
            ClassMembersMap[className] = abbreviations;
        }

        /// <summary>
        ///     Replaces references in the given regular expression string using the given reference map.
        /// </summary>
        /// <param name="regexPattern">
        ///     The string that may contain references to other regular expression patterns.
        /// </param>
        /// <param name="referenceMap">A map of reference name to regular expression pattern strings.</param>
        /// <returns>The modified regular expression string.</returns>
        private static string ReplaceReferences(string regexPattern, IDictionary<string, string> referenceMap)
        {
            var result = regexPattern;
            var matches = ReferencesRegex.Matches(regexPattern); // searches for strings enclosed by angle brackets <>
            var references = matches.ToList();

            foreach (var reference in references)
            {
                var name = reference.Value[1..^1]; // get reference name by removing opening and closing angle brackets
                if (!referenceMap.ContainsKey(name))
                {
                    throw new ProcessingException($"The reference: {name} was not found in the reference map.");
                }

                //result = result.replaceFirst(reference.Image,
                //    string.Format("({0})", Matcher.quoteReplacement(refRegExpr)));
                // ToDo - not sure exactly what the above lines do, this is my best guess:
                var regex = new Regex(reference.Value); // the full name of the reference with angle brackets
                result = regex.Replace(result, $"({referenceMap[name]})", 1);
            }

            return result;
        }

        /// <summary>
        ///     Creates a rule that matches ALL definitions.
        /// </summary>
        /// <param name="definitionsMap">The specified definitions map.</param>
        /// <returns>A Regex matching all definitions.</returns>
        public static Regex CreateAllRule(IDictionary<string, string> definitionsMap)
        {
            IList<string> definitionsList = new List<string>(definitionsMap.Values);
            return new Regex(string.Join('|', definitionsList), RegexOptions.Compiled);
        }
    }
}