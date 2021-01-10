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

using System;
using System.Collections.Generic;

namespace NetTok.Tokenizer.Annotate
{
    /// <summary>
    ///     IAnnotatedString is an interface for annotating strings and working on them. It merges the
    ///     functionality of the Java AttributedCharacterIterator class and AttributedString.
    ///     <para>
    ///         An annotated string allows iteration through both text and related annotation information. An
    ///         annotation is a key/value pair, identified by the key. No two annotations on a given character
    ///         can have the same key.
    ///     </para>
    ///     <para>
    ///         A run with respect to an annotation is a maximum text range for which:
    ///         <ul>
    ///             <li>The annotation is undefined or null for the entire range, or</li>
    ///                 <li>the annotation value is defined and has the same non-null value for the entire range</li>
    ///         </ul>
    ///         @author Joerg Steffen, DFKI, Robert J. Lebowitz, Finaltouch IT LLC
    ///     </para>
    /// </summary>
    public interface IAnnotatedString /*: IEnumerable<char>*/
    {
        /// <summary>
        ///     Returns the index of the first character of the run with respect to the given annotation key
        ///     containing the current character.
        /// </summary>
        /// <param name="key">The annotation key.</param>
        /// <returns>The integer index value.</returns>
        int GetRunStart(string key);


        /// <summary>
        ///     Returns the index of the first character following the run with respect to the given annotation
        ///     key containing the current character.
        /// </summary>
        /// <param name="key">The annotation key.</param>
        /// <returns>The integer index value.</returns>
        int GetRunLimit(string key);


        /// <summary>
        ///     Adds an annotation to a sub-range of the string.
        /// </summary>
        /// <param name="key">The annotation key.</param>
        /// <param name="value">The annotation value.</param>
        /// <param name="beginIndex">The index of the first character of the range.</param>
        /// <param name="endIndex">The index of the character following the last character of the range.</param>
        void Annotate(string key, object value, int beginIndex, int endIndex);


        /// <summary>
        ///     Returns the annotation value of the string at the current index for the given annotation key.
        /// </summary>
        /// <param name="key">The annotation key.</param>
        /// <returns>The annotation value or {@code null} if there is no annotation with the given key at that position </returns>
        object GetAnnotation(string key);

        /// <summary>
        ///     Returns the index of the first character annotated with the given annotation key following the
        ///     run containing the current character with respect to the given annotation key.
        /// </summary>
        /// <param name="key">The annotation key.</param>
        /// <returns>The integer index value.</returns>
        int FindNextAnnotation(string key);

        /// <summary>
        ///     Returns the substring between the given indices.
        /// </summary>
        /// <param name="startIndex">The index of the first character of the range.</param>
        /// <param name="endIndex">The index of the character following the last character of the range.</param>
        /// <returns>The substring within the two specified indices.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     throws an ArgumentOutOfRangeException if beginIndex is less then 0, endIndex is greater than the length of the
        ///     string,
        ///     or beginIndex and endIndex together don't define a non-empty sub-range of the string.
        /// </exception>
        string Substring(int startIndex, int endIndex);

        /// <summary>
        ///     Returns the character from the given position without changing the index.
        /// </summary>
        /// <param name="index">
        ///     The index within the text; valid values range from GetBeginIndex to GetEndIndex;
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     An ArgumentOutOfRangeException is thrown if an invalid value is supplied .
        /// </exception>
        /// <returns>The character at the specified position or if the specified position is equal to GetEndIndex.</returns>
        char this[int index] { get; }

        /// <summary>
        ///     Returns a string representation of the annotated string with the annotation for the given
        ///     annotation key.
        /// </summary>
        /// <param name="key">The annotation key.</param>
        /// <returns>A string representation of the specified annotated string.</returns>
        string ToString(string key);

        /// <summary>
        ///     Returns the surface string of the annotated string.
        /// </summary>
        /// <returns>The surface string.</returns>
        string ToString();
        #region CharacterIterator properties
        //char First { get; }
        //char Last { get; }
        //char Next { get; }
        //char Previous { get; }
        //int BeginIndex { get; }
        //int EndIndex { get; }
        int Index { get; set; }
        int Length { get; }
        char SetIndex(int index);
      //  char Current { get; }
        #endregion
    }
}