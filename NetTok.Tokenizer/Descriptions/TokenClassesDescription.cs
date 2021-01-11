using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
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

namespace NetTok.Tokenizer.Descriptions
{
    /// <summary>
    ///     Manages the content of a token classes description file.
    ///     @author Joerg Steffen, DFKI, Robert J Lebowitz, Finaltouch IT LLC
    /// </summary>
    public class TokenClassesDescription : Description
    {
        /// <summary>
        ///     Creates a new instance of <seealso cref="TokenClassesDescription" /> for the given language.
        /// </summary>
        /// <param name="language">The specified language.</param>
        public TokenClassesDescription(string language) : base(language) { }

        public override void Load(IDictionary<string, string> macrosMap)
        {
            DefinitionsMap = new Dictionary<string, Regex>();
            RulesMap = new Dictionary<string, Regex>();
            RegExpMap = new Dictionary<Regex, string>();

            using var reader =
                new StreamReader(ResourceManager.Read($"{Language}_{Constants.TokenClasses.ClassDescriptionSuffix}"));
            // read config file to definitions start
            ReadToDefinitions(reader);
            // read definitions
            IDictionary<string, string> definitionsMap = new Dictionary<string, string>();
            LoadDefinitions(reader, macrosMap, definitionsMap);
            RulesMap[Constants.TokenClasses.AllRule] = CreateAllRule(definitionsMap);
        }
    }
}