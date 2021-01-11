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

namespace NetTok.Tokenizer.Descriptions
{
    /// <summary>
    ///     Manages the content of a punctuation description file.
    ///     @author Joerg Steffen, DFKI, Robert J Lebowitz, Finaltouch IT LLC
    /// </summary>
    public class PunctuationDescription : Description
    {
        /// <summary>
        ///     class name for opening punctuation
        /// </summary>
        public const string OpenPunct = "OPEN_PUNCT";

        /// <summary>
        ///     class name for closing punctuation
        /// </summary>
        public const string ClosePunct = "CLOSE_PUNCT";

        /// <summary>
        ///     class name for opening brackets
        /// </summary>
        public const string OpenBracket = "OPEN_BRACKET";

        /// <summary>
        ///     class name for closing brackets
        /// </summary>
        public const string CloseBracket = "CLOSE_BRACKET";

        /// <summary>
        ///     class name for terminal punctuation
        /// </summary>
        public const string TermPunct = "TERM_PUNCT";

        /// <summary>
        ///     class name for possible terminal punctuation
        /// </summary>
        public const string TermPunctP = "TERM_PUNCT_P";

        /// <summary>
        ///     name of the all punctuation rule
        /// </summary>
        protected internal const string AllRule = "ALL_PUNCT_RULE";

        /// <summary>
        ///     name of the internal punctuation rule
        /// </summary>
        protected internal const string InternalRule = "INTERNAL_PUNCT_RULE";

        /// <summary>
        ///     name of the sentence internal punctuation rule
        /// </summary>
        protected internal const string InternalTuRule = "INTERNAL_TU_PUNCT_RULE";

        /// <summary>
        ///     class name for ambiguous open/close punctuation
        /// </summary>
        protected internal const string OpenClosePunct = "OPENCLOSE_PUNCT";

        /// <summary>
        ///     name suffix of the resource file with the punctuation description.
        /// </summary>
        private const string PunctuationDescriptionSuffix = "punct.cfg";


        /// <summary>
        ///     Creates a new instance of <seealso cref="PunctuationDescription" /> for the given language.
        /// </summary>
        /// <param name="language">The specified language.</param>
        /// <param name="macrosMap">A map of macro names to regular expression strings.</param>
        /// <exception cref="IOException">If there is an error when reading the configuration </exception>
        public PunctuationDescription(string language, IDictionary<string, string> macrosMap)
        {
            DefinitionsMap = new Dictionary<string, Regex>();
            RulesMap = new Dictionary<string, Regex>();
            RegExpMap = new Dictionary<Regex, string>();

            using var stream = ResourceManager.Read(language, PunctuationDescriptionSuffix);
            using var reader = new StreamReader(stream);
            // read config file to definitions start
            ReadToDefinitions(reader);
            // read definitions
            IDictionary<string, string> definitionsMap = new Dictionary<string, string>();
            base.LoadDefinitions(reader, macrosMap, definitionsMap);
            // when loadDefinitions returns the reader has reached the rules section;
            base.LoadRules(reader, definitionsMap, macrosMap);
            RulesMap[AllRule] = CreateAllRule(definitionsMap);
        }
    }
}