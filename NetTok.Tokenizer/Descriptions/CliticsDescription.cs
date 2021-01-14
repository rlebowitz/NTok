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
    ///     Manages the content of a clitics description file.
    ///     @author Joerg Steffen, DFKI, Robert J Lebowitz, Finaltouch IT LLC
    /// </summary>
    public class CliticsDescription : Description
    {
        /// <summary>
        ///     Creates a new instance of <seealso cref="CliticsDescription" /> for the given language.
        /// </summary>
        /// <param name="language">
        ///     the language
        /// </param>
        public CliticsDescription(string language) : base(language) { }

        public override void Load(IDictionary<string, string> macrosMap)
        {
            using var reader =
                new StreamReader(ResourceManager.Read($"{Language}_{Constants.Clitics.DescriptionSuffix}"));
            // read config file to definitions start
            ReadToDefinitions(reader);
            // read definitions
            var definitionsMap = new Dictionary<string, string>();
            LoadDefinitions(reader, macrosMap, definitionsMap);
            // when loadDefinitions returns the reader has reached the rules section; read rules
            LoadRules(reader, macrosMap, definitionsMap);
        }
    }
}