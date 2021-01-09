using System.Collections.Generic;
using NetTok.Tokenizer.Annotate;

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
    ///     <seealso cref="Outputter" /> provides static methods that convert an <seealso cref="IAnnotatedString" /> into a
    ///     list of
    ///     nested representation of <seealso cref="Paragraph" />s with <seealso cref="TextUnit" />s and
    ///     <seealso cref="Token" />s.
    ///     @author Joerg Steffen, DFKI
    /// </summary>
    public static class Outputter
    {
        /// <summary>
        ///     Creates a list of <seealso cref="Paragraph" />s with <seealso cref="TextUnit" />s and <seealso cref="Token" />s
        ///     from the given
        ///     annotated string.
        /// </summary>
        /// <param name="input">
        ///     the annotated string
        /// </param>
        /// <returns> a list of paragraphs </returns>
        public static List<Paragraph> CreateParagraphs(IAnnotatedString input)
        {
            // init lists for paragraphs, text units and tokens
            var paraList = new List<Paragraph>();
            var tuList = new List<TextUnit>();
            var tokenList = new List<Token>();

            // iterate over tokens and create token instances
            var c = input.SetIndex(0);
            while (c != default)
            {
                var tokenStart = input.GetRunStart(NTok.ClassAnnotation);
                var tokenEnd = input.GetRunLimit(NTok.ClassAnnotation);
                // check if c belongs to a token
                var type = (string) input.GetAnnotation(NTok.ClassAnnotation);
                if (null != type)
                {
                    // create new token instance
                    var tok = new Token(tokenStart, tokenEnd, type, input.SubString(tokenStart, tokenEnd - tokenStart));

                    // check if token is first token of a paragraph or text unit
                    if (null != input.GetAnnotation(NTok.BorderAnnotation))
                    {
                        // add current text unit to paragraph and create new one
                        tuList.Add(new TextUnit(tokenList));
                        tokenList = new List<Token>();
                    }

                    // check if token is first token of a paragraph
                    if ((string) input.GetAnnotation(NTok.BorderAnnotation) == NTok.PBorder)
                    {
                        // add current paragraph to result list and create new one
                        paraList.Add(new Paragraph(tuList));
                        tuList = new List<TextUnit>();
                    }

                    // add token to token list
                    tokenList.Add(tok);
                }

                // set iterator to next token
                c = input.SetIndex(tokenEnd);
            }

            // add last text unit
            tuList.Add(new TextUnit(tokenList));

            // add last paragraph
            paraList.Add(new Paragraph(tuList));

            // return paragraph list
            return paraList;
        }


        /// <summary>
        ///     Creates a list of <seealso cref="Token" />s from the given annotated string. Text units and paragraphs are
        ///     ignored.
        /// </summary>
        /// <param name="input">
        ///     the annotated string
        /// </param>
        /// <returns> a list of tokens </returns>
        public static IList<Token> CreateTokens(IAnnotatedString input)
        {
            // init list for tokens
            var tokenList = new List<Token>();

            // iterate over tokens and create token instances
            var c = input.SetIndex(0);
            while (c != default)
            {
                var tokenStart = input.GetRunStart(NTok.ClassAnnotation);
                var tokenEnd = input.GetRunLimit(NTok.ClassAnnotation);
                // check if c belongs to a token
                var type = (string) input.GetAnnotation(NTok.ClassAnnotation);
                if (null != type)
                {
                    // create new token instance
                    var tok = new Token(tokenStart, tokenEnd, type, input.SubString(tokenStart, tokenEnd - tokenStart));

                    // add token to token list
                    tokenList.Add(tok);
                }

                // set iterator to next token
                c = input.SetIndex(tokenEnd);
            }

            // return paragraph list
            return tokenList;
        }
    }
}