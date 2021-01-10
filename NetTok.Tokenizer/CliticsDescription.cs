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
    ///     Manages the content of a clitics description file.
    ///     @author Joerg Steffen, DFKI, Robert J Lebowitz, Finaltouch IT LLC
    /// </summary>
    public class CliticsDescription : Description
    {
        /// <summary>
        ///     Name of the proclitic rule.
        /// </summary>
        protected internal const string ProcliticRule = "PROCLITIC_RULE";

        /// <summary>
        ///     name of the enclitic rule
        /// </summary>
        protected internal const string EncliticRule = "ENCLITIC_RULE";

        // name suffix of the resource file with the clitic description
        private const string CliticDescriptionSuffix = "clitics.cfg";


        /// <summary>
        ///     Creates a new instance of <seealso cref="CliticsDescription" /> for the given language.
        /// </summary>
        /// <param name="language">
        ///     the language
        /// </param>
        /// <param name="macrosMap">
        ///     a map of macro names to regular expression strings
        /// </param>
        /// <exception cref="IOException">
        ///     if there is an error when reading the configuration
        /// </exception>
        public CliticsDescription(string language, IDictionary<string, string> macrosMap)
        {
            DefinitionsMap = new Dictionary<string, Regex>();
            RulesMap = new Dictionary<string, Regex>();
            RegExpMap = new Dictionary<Regex, string>();

            using var stream = ResourceManager.Read(language, CliticDescriptionSuffix);
            using var reader = new StreamReader(stream);

            // read config file to definitions start
            ReadToDefinitions(reader);
            // read definitions
            IDictionary<string, string> definitionsMap = new Dictionary<string, string>();
            base.LoadDefinitions(reader, macrosMap, definitionsMap);
            // when loadDefinitions returns the reader has reached the rules section; read rules
            base.LoadRules(reader, definitionsMap, macrosMap);
        }
    }
}