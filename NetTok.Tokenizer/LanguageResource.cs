using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using NetTok.Tokenizer.Descriptions;
using NetTok.Tokenizer.Exceptions;

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
        // name suffix of the resource file with the classes hierarchy
        private const string ClassesHierarchy = "class_hierarchy.xml";

        // name suffix of the config file with the macros
        private const string MacrosConfiguration = "macros.cfg";

        /// <summary>
        ///     Creates a new instance of <seealso cref="LanguageResource" /> for the given language using the resource
        ///     description files in the given resource directory.
        /// </summary>
        /// <param name="language">
        ///     the name of the language for which this class contains the resources
        /// </param>
        /// <exception cref="InitializationException">
        ///     if an error occurs
        /// </exception>
        public LanguageResource(string language)
        {
            // init stuff
            AncestorsMap = new Dictionary<string, List<string>>();
            PunctuationDescription = null;
            CliticsDescription = null;
            AbbreviationDescription = null;
            ClassesDescription = null;
            Language = language ?? "default";

            try
            {
                // load classes hierarchy
                using var stream = ResourceManager.Read(language, ClassesHierarchy);
                var document = XDocument.Parse(new StreamReader(stream).ReadToEnd());
                // set hierarchy root
                ClassesRoot = document.Root;
                // map class names to dom elements
                MapSingleClass(ClassesRoot);
                MapClasses(ClassesRoot.Elements().ToList());
                // load macros
                var macrosMap = new Dictionary<string, string>();
                Description.LoadMacros(Language, MacrosConfiguration, macrosMap);

                // load punctuation description
                PunctuationDescription = new PunctuationDescription(language, macrosMap);

                // load clitics description
                CliticsDescription = new CliticsDescription(language, macrosMap);

                // load abbreviation description
                AbbreviationDescription = new AbbreviationDescription(language, macrosMap);

                // load token classes description document
                ClassesDescription = new TokenClassesDescription(language, macrosMap);
            }
            catch (IOException ioe)
            {
                throw new InitializationException(ioe.Message, ioe);
            }
        }


        // name of the language for which this class contains the resources

        // root element of the classes hierarchy

        // name of the root element of the classes hierarchy
        public string ClassesRootName => ClassesRoot?.TagName();

        public string Language { get; }
        public XElement ClassesRoot { get; set; }
        public IDictionary<string, List<string>> AncestorsMap { get; set; }
        public PunctuationDescription PunctuationDescription { get; set; }
        public CliticsDescription CliticsDescription { get; set; }
        public AbbreviationDescription AbbreviationDescription { get; set; }
        public TokenClassesDescription ClassesDescription { get; set; }

        /// <summary>
        ///     The Regular Expression matcher for all punctuation from the punctuation description.
        /// </summary>
        public Regex AllPunctuationMatcher => PunctuationDescription.RulesMap[PunctuationDescription.AllRule];

        /// <summary>The matcher for internal punctuation from the punctuation description.</summary>
        public Regex InternalMatcher => PunctuationDescription.RulesMap[PunctuationDescription.InternalRule];

        /// <summary>The matcher for sentence internal punctuation from the punctuation description.</summary>
        public Regex InternalTuMatcher => PunctuationDescription.RulesMap[PunctuationDescription.InternalTuRule];

        /// <summary>The matcher for pro-clitics from the clitics description.</summary>
        public Regex ProcliticsMatcher => CliticsDescription.RulesMap[CliticsDescription.ProcliticRule];

        /// <summary>The matcher for enclitics from the clitics description.</summary>
        public Regex EncliticsMatcher => CliticsDescription.RulesMap[CliticsDescription.EncliticRule];

        /// <summary>The map with the abbreviation lists.</summary>
        public IDictionary<string, HashSet<string>> AbbreviationLists => AbbreviationDescription.ClassMembersMap;

        /// <summary>The matcher for the all abbreviations from the abbreviations description.</summary>
        public Regex AllAbbreviationMatcher => AbbreviationDescription.RulesMap[AbbreviationDescription.AllRule];

        /// <summary>
        ///     The set of the most common terms that only start with a capital letter when they are at the beginning of a sentence
        /// </summary>
        public HashSet<string> NonCapitalizedTerms => AbbreviationDescription.NonCapTerms;


        /// <summary> the matcher for all token classes from the token classes description </summary>
        public Regex AllClassesMatcher => ClassesDescription.RulesMap[TokenClassesDescription.AllRule];

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
            var key = element.TagName();
            // collect ancestors of element
            var ancestors = new List<string>();
            while (element.Parent != null && element.Parent != ClassesRoot)
            {
                ancestors.Add(element.Parent.TagName());
                element = element.Parent;
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