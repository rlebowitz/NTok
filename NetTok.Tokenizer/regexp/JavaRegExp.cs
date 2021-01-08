using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

/*
 * JTok
 * A configurable tokenizer implemented in Java
 *
 * (C) 2003 - 2014  DFKI Language Technology Lab http://www.dfki.de/lt
 *   Author: Joerg Steffen, steffen@dfki.de
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

namespace NetTok.Tokenizer.RegExp
{
    /// <summary>
    ///     Implements the <seealso cref="IRegExp" /> interface for regular expressions of the java.util.regex package.
    ///     @author Joerg Steffen, DFKI
    /// </summary>
    public class JavaRegExp : IRegExp
    {
        // instance of a regular expression in the java.util.regex package
        private readonly Regex regex;


        /// <summary>
        ///     Creates a new instance of <seealso cref="JavaRegExp" /> for the given regular expression string.
        /// </summary>
        /// <param name="regExpString">A regular expression string.</param>
        public JavaRegExp(string regExpString)
        {
            Guard.NotNull(regExpString);
            regex = new Regex(regExpString, RegexOptions.Compiled);
        }

        //https://stackoverflow.com/questions/12730251/convert-result-of-matches-from-regex-into-list-of-string
        public virtual List<Match> GetAllMatches(string input)
        {
            var matches = regex.Matches(input);
            return matches.Select(match => match).ToList();
        }

        public virtual bool Matches(string input)
        {
            return regex.IsMatch(input);
        }

        public virtual Match Contains(string input)
        {
            if (regex.IsMatch(input))
            {
                return regex.Match(input);
            }

            return null;
        }

        public virtual Match Starts(string input)
        {
            if (!regex.IsMatch(input))
            {
                return null;
            }

            var match = regex.Match(input);
            return match.Index == 0 ? match : null;
        }

        public virtual Match Ends(string input)
        {
            if (regex.IsMatch(input))
            {
                var matches = regex.Matches(input);
                var lastMatch = matches.Last();
                if (lastMatch.Index == input.Length - lastMatch.Length)
                {
                    return lastMatch;
                }
            }

            return null;
        }

        public override string ToString()
        {
            return regex.ToString();
        }
    }
}