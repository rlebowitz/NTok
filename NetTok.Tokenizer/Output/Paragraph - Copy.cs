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
    ///     Represents a paragraph with its text units.
    ///     @author Joerg Steffen, DFKI, Robert J Lebowitz, Finaltouch IT LLC
    /// </summary>
    public class NewParagraph
    {
        /// <summary>
        ///     Creates a new instance of <seealso cref="Paragraph" />.
        /// </summary>
        public NewParagraph()
        {
            StartIndex = 0;
            EndIndex = 0;
            TextUnits = new List<TextUnit>();
        }

        public NewParagraph(string s, int offset)
        {
            StartIndex = offset;
            EndIndex = offset + s.Length;
            TextUnits = new List<TextUnit>();
        }

        /// <summary>
        ///     Creates a new instance of <seealso cref="Paragraph" /> that contains the given text units.
        /// </summary>
        /// <param name="textUnits">
        ///     a list of text units
        /// </param>
        public void Paragraph(IList<TextUnit> textUnits)
        {
            TextUnits = textUnits;
        }

        public IList<TextUnit> TextUnits { get; private set; }

        /// <summary>The start index.</summary>
        public int StartIndex { get; set; }


        /// <summary>The end index.</summary>
        public int EndIndex { get; set; }

       

        public override string ToString()
        {
            var result =
                new StringBuilder($"Paragraph Start: {StartIndex}{Environment.NewLine}Paragraph End: {EndIndex}{Environment.NewLine}");
            foreach (var unit in TextUnits)
            {
                result.Append(unit);
            }

            return result.ToString();
        }
    }
}