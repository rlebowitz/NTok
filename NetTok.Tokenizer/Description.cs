﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NetTok.Tokenizer.Exceptions;
using NetTok.Tokenizer.regexp;

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

        /// <summary>
        ///     factory for creating regular expressions
        /// </summary>
        public static readonly RegExpFactory Factory = new DkBricsRegExpFactory();


        // regular expression for matching references used in regular expressions of config files
        private static readonly RegExp ReferencesMatcher = Factory.createRegExp("\\<[A-Za-z0-9_]+\\>");

        /// <returns> the definitions map </returns>
        public Dictionary<string, RegExp> DefinitionsMap { get; set; }

        /// <returns> the rules map </returns>
        public Dictionary<string, RegExp> RulesMap { get; set; }

        /// <returns> the regular expressions map </returns>
        public Dictionary<RegExp, string> RegExpMap { get; set; }

        /// <returns> the class members map </returns>
        public virtual Dictionary<string, HashSet<string>> ClassMembersMap { get; set; }
        ///// <summary>
        /////     Returns the first child element of the given element with the given name. If no such child
        /////     exists, returns {@code null}.
        ///// </summary>
        ///// <param name="ele">
        /////     the parent element
        ///// </param>
        ///// <param name="childName">
        /////     the child name
        ///// </param>
        ///// <returns> the first child element with the specified name or {@code null} if no such child exists </returns>
        //public Element getChild(XElement ele, string childName)
        //{
        //    NodeList children = ele.ChildNodes;
        //    for (int i = 0, iMax = children.Length; i < iMax; i++)
        //    {
        //        Node oneChild = children.item(i);
        //        if (oneChild is Element && ((Element) oneChild).TagName.Equals(childName))
        //        {
        //            return (Element) oneChild;
        //        }
        //    }

        //    return null;
        //}


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
            var s = ResourceMethods.ReadResource(language, fileName);
            var @in = new StringReader(s);
            string line;
            while ((line = @in.ReadLine()) != null)
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
        /// <param name="in">
        ///     the reader
        /// </param>
        /// <exception cref="IOException">
        ///     if there is an error when reading
        /// </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected static void readToLists(java.io.BufferedReader in) throws java.io.IOException
        public static void readToLists(StreamReader @in)
        {
            if (null == @in)
            {
                return;
            }

            string line;
            while (!ReferenceEquals(line = @in.ReadLine(), null))
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
        /// <param name="in">
        ///     the reader
        /// </param>
        /// <exception cref="IOException">
        ///     if there is an error when reading
        /// </exception>
        public static void ReadToDefinitions(StreamReader @in)
        {
            if (null == @in)
            {
                return;
            }

            string line;
            while (!ReferenceEquals(line = @in.ReadLine(), null))
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
        ///     <br>
        ///         Immediately returns if the reader is {@code null}.
        /// </summary>
        /// <param name="in">
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
        
        public virtual void LoadDefinitions(StreamReader @in, IDictionary<string, string> macrosMap,
            IDictionary<string, string> defMap)
        {
            if (null == @in)
            {
                return;
            }

            // init temporary map where to store the regular expression string
            // for each class
            IDictionary<string, StringBuilder> tempMap = new Dictionary<string, StringBuilder>();

            string line;
            while (!ReferenceEquals(line = @in.ReadLine(), null))
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
                    throw new InitializationException(string.Format("missing separator in definitions section line {0}",
                        line));
                }

                var defName = line.Substring(0, firstSep).Trim();
                var regExpString = line.Substring(firstSep + 1, secondSep - (firstSep + 1)).Trim();
                var className = line.Substring(secondSep + 1).Trim();

                // expand possible macros
                regExpString = ReplaceReferences(regExpString, macrosMap);

                // check for empty regular expression
                if (regExpString.Length == 0)
                {
                    throw new ProcessingException(string.Format("empty regular expression in line {0}", line));
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
                    DefinitionsMap[oneEntry.Key] = Factory.createRegExp(oneEntry.Value.ToString());
                }
                catch (Exception e)
                {
                    throw new ProcessingException(
                        $"cannot create regular expression for {oneEntry.Key} from {oneEntry.Value}: {e.Message}");
                }
            }
        }


        /// <summary>
        ///     Reads the rules section from the given reader to map each rules to a regular expression that
        ///     matches all tokens of that rule.
        ///     <br>
        ///         Immediately returns if the reader is {@code null}.
        /// </summary>
        /// <param name="in">
        ///     the reader
        /// </param>
        /// <param name="defsMap">
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
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void loadRules(java.io.BufferedReader in, java.util.Map<String, String> defsMap, java.util.Map<String, String> macrosMap) throws java.io.IOException
        public virtual void loadRules(StreamReader @in, IDictionary<string, string> defsMap,
            IDictionary<string, string> macrosMap)
        {
            if (null == @in)
            {
                return;
            }

            string line;
            while (!ReferenceEquals(line = @in.ReadLine(), null))
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
                        string.Format("missing separator in rules section line {0}", line));
                }

                var ruleName = line.Substring(0, firstSep).Trim();
                var regExpString = line.Substring(firstSep + 1, secondSep - (firstSep + 1)).Trim();
                var className = line.Substring(secondSep + 1).Trim();

                // expand definitions
                regExpString = ReplaceReferences(regExpString, defsMap);
                // expand possible macros
                regExpString = ReplaceReferences(regExpString, macrosMap);

                // add rule to map
                var regExp = Factory.createRegExp(regExpString);
                RulesMap[ruleName] = regExp;
                // if rule has a class, add regular expression to regular expression map
                if (className.Length > 0)
                {
                    RegExpMap[regExp] = className;
                }
            }
        }


        /// <summary>
        ///     Reads the lists section from the given reader to map each token class from the lists to a set
        ///     that contains all members of that class.
        ///     <br>
        ///         Immediately returns if the reader is {@code null}.
        /// </summary>
        /// <param name="in">
        ///     the reader
        /// </param>
        /// <param name="resourceDir">
        ///     the resource directory
        /// </param>
        /// <exception cref="IOException">
        ///     if there is an error during reading
        /// </exception>
        /// <exception cref="InitializationException">
        ///     if configuration fails
        /// </exception>
        public void LoadLists(StreamReader @in, string resourceDir)
        {
            if (null == @in)
            {
                return;
            }

            string line;
            while ((line = @in.ReadLine()) != null)
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

                var sep = line.IndexOf(":", StringComparison.Ordinal);
                if (sep == -1)
                {
                    throw new InitializationException(
                        string.Format("missing separator in lists section line {0}", line));
                }

                var listFileName = line.Substring(0, sep).Trim();
                var className = line.Substring(sep + 1).Trim();
                LoadList(Paths.get(resourceDir).resolve(listFileName), className);
            }
        }


        /// <summary>
        ///     Loads the abbreviations list from the given path and stores its items under the given classname
        /// </summary>
        /// <param name="listPath">
        ///     the abbreviations list path
        /// </param>
        /// <param name="className">
        ///     the class name
        /// </param>
        /// <exception cref="IOException">
        ///     if there is an error when reading the list
        /// </exception>
        private void LoadList(string className)
        {
            ResourceMethods.ReadResource()
            var @in = new StreamReader(FileTools.openResourceFileAsStream(listPath), Encoding.UTF8);
            // init set where to store the abbreviations
            var items = new HashSet<string>();
            // iterate over lines of file
            string line;
            while ((line = @in.ReadLine()) != null)
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

            @in.Close();
            // add set to lists map
            ClassMembersMap[className] = items;
        }


        /// <summary>
        ///     Replaces references in the given regular expression string using the given reference map.
        /// </summary>
        /// <param name="regExpString">
        ///     the regular expression string with possible references
        /// </param>
        /// <param name="refMap">
        ///     a map of reference name to regular expression strings
        /// </param>
        /// <returns> the modified regular expression string </returns>
        private static string ReplaceReferences(string regExpString, IDictionary<string, string> refMap)
        {
            var result = regExpString;

            var references = ReferencesMatcher.getAllMatches(regExpString);

            foreach (var oneRef in references)
            {
                // get reference name by removing opening and closing angle brackets
                var refName = oneRef.Image.Substring(1, oneRef.Image.Length - 1 - 1);
                var refRegExpr = refMap[refName];
                if (null == refRegExpr)
                {
                    throw new ProcessingException(string.Format("unknown reference {0} in regular expression {1}",
                        refName, regExpString));
                }

                result = result.replaceFirst(oneRef.Image,
                    string.Format("({0})", Matcher.quoteReplacement(refRegExpr)));
            }

            return result;
        }


        /// <summary>
        ///     Creates a rule that matches ALL definitions.
        /// </summary>
        /// <param name="defsMap">
        ///     the definitions map
        /// </param>
        /// <returns> a regular expression matching all definitions </returns>
        public static RegExp createAllRule(IDictionary<string, string> defsMap)
        {
            var ruleRegExpr = new StringBuilder();

            // iterate over definitions
            IList<string> defsList = new List<string>(defsMap.Values);
            for (int i = 0, iMax = defsList.Count; i < iMax; i++)
            {
                var regExpr = defsList[i];
                // extend regular expression with another disjunct
                ruleRegExpr.Append(string.Format("({0})", regExpr));
                if (i < iMax - 1)
                {
                    ruleRegExpr.Append("|");
                }
            }

            return Factory.createRegExp(ruleRegExpr.ToString());
        }
    }
}