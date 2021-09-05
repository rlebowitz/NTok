using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using NetTok.Tokenizer.Descriptions;
using NetTok.Tokenizer.Exceptions;
using NetTok.Tokenizer.Utilities;

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
    ///     Manages the language-specific information needed by the tokenizer to process a document of that
    ///     language.
    ///     @author Joerg Steffen, DFKI
    /// </summary>
    public class LanguageResource
    {
        /// <summary>
        ///     Creates a new instance of LanguageResource for the specified language using the resource
        ///     description files in the given resource directory.
        /// </summary>
        /// <param name="language">
        ///     The specified name of the language for which to load resources.
        /// </param>
        /// <remarks>
        ///     If the specified language is null or non-existent, the language used will be specified as 'default'.
        /// </remarks>
        /// <exception cref="InitializationException">
        ///     Thrown if an error occurs while loading any of the various embedded resource files.
        /// </exception>
        public LanguageResource(string language)
        {
            Language = language ?? "default";
            AncestorsMap = new Dictionary<string, List<string>>();
            MacroDescription = new MacroDescription(Language);
            PunctuationDescription = new PunctuationDescription(Language);
            CliticsDescription = new CliticsDescription(Language);
            AbbreviationDescription = new AbbreviationDescription(Language);
            ClassesDescription = new TokenClassesDescription(Language);

            try
            {
                // load classes hierarchy
                using var reader =
                    new StreamReader(ResourceManager.Read($"{Language}_{Constants.Resources.ClassesHierarchy}"));
                var document = XDocument.Parse(reader.ReadToEnd());
                // set hierarchy root
                ClassesRoot = document.Root;
                // map class names to dom elements
                MapSingleClass(ClassesRoot);
                MapClasses(ClassesRoot.Elements().ToList());
                // load macros
                var macrosMap = new Dictionary<string, string>();
                MacroDescription.Load(macrosMap);
                // load punctuation description
                PunctuationDescription.Load(macrosMap);
                // load clitics description
                CliticsDescription.Load(macrosMap);
                // load abbreviation description
                AbbreviationDescription.Load(macrosMap);
                // load token classes description document
                ClassesDescription.Load(macrosMap);
            }
            catch (Exception ex)
            {
                throw new InitializationException(ex.Message, ex);
            }
        }

        /// <summary>
        ///     Tag name of the root element of the classes hierarchy.
        /// </summary>
        public string ClassesRootName => ClassesRoot?.TagName();

        public string Language { get; }
        public XElement ClassesRoot { get; set; }
        public IDictionary<string, List<string>> AncestorsMap { get; set; }
        public MacroDescription MacroDescription { get; set; }
        public PunctuationDescription PunctuationDescription { get; set; }
        public CliticsDescription CliticsDescription { get; set; }
        public AbbreviationDescription AbbreviationDescription { get; set; }
        public TokenClassesDescription ClassesDescription { get; set; }

        /// <summary>
        ///     The Regular Expression matcher for all punctuation from the punctuation description.
        /// </summary>
        public Regex AllPunctuationMatcher => PunctuationDescription.RulesMap[Constants.Punctuation.AllRule];

        /// <summary>The matcher for internal punctuation from the punctuation description.</summary>
        public Regex InternalMatcher => PunctuationDescription.RulesMap[Constants.Punctuation.InternalRule];

        /// <summary>The matcher for sentence internal punctuation from the punctuation description.</summary>
        public Regex InternalTuMatcher => PunctuationDescription.RulesMap[Constants.Punctuation.InternalTuRule];

        /// <summary>The matcher for pro-clitics from the clitics description.</summary>
        public Regex ProcliticsMatcher => CliticsDescription.RulesMap[Constants.Clitics.ProcliticRule];

        /// <summary>The matcher for enclitics from the clitics description.</summary>
        public Regex EncliticsMatcher => CliticsDescription.RulesMap[Constants.Clitics.EncliticRule];

        /// <summary>The map with the abbreviation lists.</summary>
        public IDictionary<string, HashSet<string>> AbbreviationMap => AbbreviationDescription.ClassMembersMap;

        /// <summary>The matcher for the all abbreviations from the abbreviations description.</summary>
        public Regex AllAbbreviationMatcher => AbbreviationDescription.RulesMap[Constants.Abbreviations.AllRule];

        /// <summary>
        ///     The set of the most common terms that only start with a capital letter when they are at the beginning of a sentence
        /// </summary>
        public HashSet<string> NonCapitalizedTerms => AbbreviationDescription.NonCapTerms;

        /// <summary> the matcher for all token classes from the token classes description </summary>
        public Regex AllClassesMatcher => ClassesDescription.RulesMap[Constants.TokenClasses.AllRule];

        /// <summary>
        ///     Iterates recursively over a list of class elements and adds each element's ancestors to
        ///     ancestors map using the name of the element as key.
        /// </summary>
        /// <param name="elementList">
        ///     node list of class elements
        /// </param>
        private void MapClasses(IEnumerable<XElement> elementList)
        {
            // iterate over elements
            foreach (var element in elementList)
            {
                MapSingleClass(element);
                // add children of element to maps
                if (element.Elements().Any())
                {
                    MapClasses(element.Elements().ToList());
                }
            }
        }

        /// <summary>
        ///     Creates mappings for the given class in the ancestor maps.
        /// </summary>
        /// <param name="element">A class element.</param>
        private void MapSingleClass(XElement element)
        {
            var tag = element;
            var key = element.TagName();
            // collect ancestors of element
            var ancestors = new List<string>();
            while (tag.Parent != null && tag.Parent != ClassesRoot)
            {
                ancestors.Add(tag.Parent.TagName());
                tag = tag.Parent;
            }

            // add list to ancestors map
            AncestorsMap[key] = ancestors;
        }

        /// <summary>
        ///     Checks if the first given class is ancestor in the class hierarchy of the second given class
        ///     or equals the second given class.
        /// </summary>
        /// <param name="class1">The first class name.</param>
        /// <param name="class2">The second class name.</param>
        /// <returns>True if the first class is the ancestor of the second class, otherwise false.</returns>
        /// <exception cref="ProcessingException">if the second class name is not a defined class.</exception>
        public virtual bool IsAncestor(string class1, string class2)
        {
            if (class1.Equals(ClassesRootName) || class1.Equals(class2))
            {
                return true;
            }

            return AncestorsMap.ContainsKey(class2) ? AncestorsMap.ContainsKey(class1) : class1.Equals(ClassesRootName);
        }
    }
}