using System;
using System.Collections.Generic;
using System.Text;

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

namespace NetTok.Tokenizer.Output
{
    /// <summary>
    ///     Represents a text unit with its tokens.
    ///     @author Joerg Steffen, DFKI, Robert J Lebowitz, Finaltouch IT LLC
    /// </summary>
    public class TextUnit
    {
        /// <summary>
        ///     Creates a new instance of <seealso cref="TextUnit" />.
        /// </summary>
        public TextUnit()
        {
            StartIndex = 0;
            EndIndex = 0;
            Tokens = new List<Token>();
        }

        /// <summary>
        ///     Creates a new instance of TextUnit" containing the given tokens.
        /// </summary>
        /// <param name="tokens">A list of tokens.</param>
        public TextUnit(IList<Token> tokens)
        {
            Tokens = tokens;
        }

        // start index of the text unit

        // end index of the text unit

        // list with the tokens of the text unit
        private IList<Token> TokenList { get; set; }

        /// <summary>The start index.</summary>
        public int StartIndex { get; set; }

        /// <summary>The end index.</summary>
        public int EndIndex { get; set; }

        /// <summary>The token list.</summary>
        public IList<Token> Tokens
        {
            get => TokenList;
            set
            {
                TokenList = value;
                if (value.Count > 0)
                {
                    StartIndex = value[0].StartIndex;
                    EndIndex = value[^1].EndIndex;
                }
                else
                {
                    StartIndex = 0;
                    EndIndex = 0;
                }
            }
        }

        public override string ToString()
        {
            var result =
                new StringBuilder(
                    $"  Text Unit Start: {StartIndex}{Environment.NewLine}  Text Unit End: {EndIndex}{Environment.NewLine}");
            // add tokens
            foreach (var token in Tokens)
            {
                result.Append(token);
            }

            return result.ToString();
        }
    }
}