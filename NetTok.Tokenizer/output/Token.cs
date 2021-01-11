using System.Text;
using NetTok.Tokenizer.Descriptions;
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

namespace NetTok.Tokenizer.Output
{
    /// <summary>
    ///     Represents a token with its type and surface image.
    ///     @author Joerg Steffen, DFKI, Robert J Lebowitz, Finaltouch IT LLC
    /// </summary>
    public class Token
    {
        // the Penn Treebank replacements for brackets:
        private const string LRB = "-LRB-";
        private const string RRB = "-RRB-";
        private const string LSB = "-LSB-";
        private const string RSB = "-RSB-";
        private const string LCB = "-LCB-";
        private const string RCB = "-RCB-";

        public Token() { }

        /// <summary>
        ///     Creates a new instance of Token for the given start index, end index, type and surface image.
        /// </summary>
        /// <param name="startIndex">The specified start index.</param>
        /// <param name="endIndex">The specified end index.</param>
        /// <param name="type">The specified type.</param>
        /// <param name="image">The surface image.</param>
        public Token(int startIndex, int endIndex, string type, string image)
        {
            StartIndex = startIndex;
            EndIndex = endIndex;
            Type = type;
            Image = image;
        }

        /// <summary>The start index.</summary>
        public int StartIndex { get; }

        /// <summary>The end index.</summary>
        public int EndIndex { get; }

        /// <summary>The token type.</summary>
        public string Type { get; } = string.Empty;

        /// <summary>The surface image.</summary>
        public string Image { get; } = string.Empty;

        /// <summary>
        ///     Returns the Penn Treebank surface image of the token if a Penn Treebank replacement took place,
        ///     otherwise returns null.
        /// </summary>
        /// <returns>The surface image as the result of the Penn Treebank token replacement or null.</returns>
        public string PennTreeBankImage => ApplyPennTreeBankFormat(Image, Type);


        public override string ToString()
        {
            var result = new StringBuilder(
                $"    Token: {$"\"{Image}\"",-15}\tType: {Type}\tStart: {StartIndex}\tEnd: {EndIndex}");

            var ptbImage = ApplyPennTreeBankFormat(Image, Type);
            if (null != ptbImage)
            {
                result.Append($"\tPTB: \"{ptbImage}\"");
            }

            result.Append("%n");

            return result.ToString();
        }


        /// <summary>
        ///     This applies some replacements used in the Penn Treebank format to the given token image of the given type.
        /// </summary>
        /// <param name="image">The token image.</param>
        /// <param name="type">The token type.</param>
        /// <returns>A modified string or {@code null} if no replacement took place.</returns>
        public static string ApplyPennTreeBankFormat(string image, string type)
        {
            string result = null;

            switch (type)
            {
                case Constants.Punctuation.OpenBracket:
                    result = image switch
                    {
                        "(" => LRB,
                        "[" => LSB,
                        "{" => LCB,
                        _ => null
                    };
                    break;
                case Constants.Punctuation.CloseBracket:
                    result = image switch
                    {
                        ")" => RRB,
                        "]" => RSB,
                        "}" => RCB,
                        _ => null
                    };
                    break;
                case Constants.Punctuation.OpenPunct:
                    result = "``";
                    break;
                case Constants.Punctuation.ClosePunct:
                    result = "''";
                    break;
                default:
                {
                    if (image.Contains("/"))
                    {
                        result = image.Replace("/", "\\/");
                    }
                    else if (image.Contains("*"))
                    {
                        result = image.Replace("*", "\\*");
                    }

                    break;
                }
            }

            return result;
        }
    }
}