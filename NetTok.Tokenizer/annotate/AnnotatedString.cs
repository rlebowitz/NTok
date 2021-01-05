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

namespace NetTok.Tokenizer.annotate
{

	/// <summary>
	/// <seealso cref="AnnotatedString"/> is an interface for annotating strings and working on them. It merges the
	/// functionality of <seealso cref="java.text.AttributedCharacterIterator"/> and
	/// <seealso cref="java.text.AttributedString"/>.
	/// <para>
	/// An annotated string allows iteration through both text and related annotation information. An
	/// annotation is a key/value pair, identified by the key. No two annotations on a given character
	/// can have the same key.
	/// </para>
	/// <para>
	/// A run with respect to an annotation is a maximum text range for which:
	/// <ul>
	/// <li>the annotation is undefined or null for the entire range, or
	/// <li>the annotation value is defined and has the same non-null value for the entire range
	/// </ul>
	/// 
	/// @author Joerg Steffen, DFKI
	/// </para>
	/// </summary>
	public interface AnnotatedString : CharacterIterator
	{

	  /// <summary>
	  /// Returns the index of the first character of the run with respect to the given annotation key
	  /// containing the current character.
	  /// </summary>
	  /// <param name="key">
	  ///          the annotation key </param>
	  /// <returns> the index </returns>
	  int getRunStart(string key);


	  /// <summary>
	  /// Returns the index of the first character following the run with respect to the given annotation
	  /// key containing the current character.
	  /// </summary>
	  /// <param name="key">
	  ///          the annotation key </param>
	  /// <returns> the index </returns>
	  int getRunLimit(string key);


	  /// <summary>
	  /// Adds an annotation to a subrange of the string.
	  /// </summary>
	  /// <param name="key">
	  ///          the annotation key </param>
	  /// <param name="value">
	  ///          the annotation value </param>
	  /// <param name="beginIndex">
	  ///          the index of the first character of the range </param>
	  /// <param name="endIndex">
	  ///          the index of the character following the last character of the range </param>
	  /// <exception cref="IllegalArgumentException">
	  ///              if beginIndex is less then 0, endIndex is greater than the length of the string,
	  ///              or beginIndex and endIndex together don't define a non-empty subrange of the
	  ///              string </exception>
	  void annotate(string key, object value, int beginIndex, int endIndex);


	  /// <summary>
	  /// Returns the annotation value of the string at the current index for the given annotation key.
	  /// </summary>
	  /// <param name="key">
	  ///          the annotation key </param>
	  /// <returns> the annotation value or {@code null} if there is no annotation with the given key at
	  ///         that position </returns>
	  object getAnnotation(string key);


	  /// <summary>
	  /// Returns the index of the first character annotated with the given annotation key following the
	  /// run containing the current character with respect to the given annotation key.
	  /// </summary>
	  /// <param name="key">
	  ///          the annotation key </param>
	  /// <returns> the index </returns>
	  int findNextAnnotation(string key);


	  /// <summary>
	  /// Returns the substring between the given indices.
	  /// </summary>
	  /// <param name="startIndex">
	  ///          the index of the first character of the range </param>
	  /// <param name="endIndex">
	  ///          the index of the character following the last character of the range </param>
	  /// <returns> the substring </returns>
	  /// <exception cref="IllegalArgumentException">
	  ///              if beginIndex is less then 0, endIndex is greater than the length of the string,
	  ///              or beginIndex and endIndex together don't define a non-empty subrange of the
	  ///              string </exception>
	  string substring(int startIndex, int endIndex);


	  /// <summary>
	  /// Returns the character from the given position without changing the index.
	  /// </summary>
	  /// <param name="charIndex">
	  ///          the index within the text; valid values range from <seealso cref="getBeginIndex()"/> to
	  ///          <seealso cref="getEndIndex()"/>; an IllegalArgumentException is thrown if an invalid value is
	  ///          supplied </param>
	  /// <returns> the character at the specified position or <seealso cref="DONE"/> if the specified position is
	  ///         equal to <seealso cref="getEndIndex()"/> </returns>
	  char charAt(int charIndex);


	  /// <summary>
	  /// Returns a string representation of the annotated string with the annotation for the given
	  /// annotation key.
	  /// </summary>
	  /// <param name="key">
	  ///          the annotation key </param>
	  /// <returns> the string representation </returns>
	  string toString(string key);


	  /// <summary>
	  /// Returns the surface string of the annotated string.
	  /// </summary>
	  /// <returns> the surface string </returns>
	  string ToString();
	}

}