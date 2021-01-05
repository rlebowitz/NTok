using System.Collections.Generic;

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

namespace NetTok.Tokenizer.regexp
{

	/// <summary>
	/// Interface for regular expression patterns.
	/// 
	/// @author Joerg Steffen, DFKI
	/// </summary>
	public interface RegExp
	{

	  /// <summary>
	  /// Returns a list with all matches for the regular expression in the given input.
	  /// </summary>
	  /// <param name="input">
	  ///          the string where to look for matches </param>
	  /// <returns> a list of matches </returns>
	  IList<Match> getAllMatches(string input);


	  /// <summary>
	  /// Checks if the regular expression matches the given input in its entirety.
	  /// </summary>
	  /// <param name="input">
	  ///          the string to check </param>
	  /// <returns> a flag indicating the match </returns>
	  bool matches(string input);


	  /// <summary>
	  /// Checks if the given input contains a match for the regular expression. If yes, the first match
	  /// is returned, {@code null} otherwise.
	  /// </summary>
	  /// <param name="input">
	  ///          the string to check </param>
	  /// <returns> a match or {@code null} </returns>
	  Match contains(string input);


	  /// <summary>
	  /// Checks if the given input contains a match for the regular expression at the start of the
	  /// input. If yes, the match is returned, {@code null} otherwise.
	  /// </summary>
	  /// <param name="input">
	  ///          the string to check </param>
	  /// <returns> a match or {@code null} </returns>
	  Match starts(string input);


	  /// <summary>
	  /// Checks if the given input contains a match for the regular expression at the end of the input.
	  /// If yes, the match is returned, {@code null} otherwise.
	  /// </summary>
	  /// <param name="input">
	  ///          the string to check </param>
	  /// <returns> a match or {@code null} </returns>
	  Match ends(string input);
	}

}