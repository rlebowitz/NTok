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

using NetTok.Tokenizer.exceptions;

namespace NetTok.Tokenizer.regexp
{
	using InitializationException = InitializationException;

	/// <summary>
	/// Abstract class for creating objects that fit the <seealso cref="RegExp"/> interface.
	/// 
	/// @author Joerg Steffen, DFKI
	/// </summary>
	public abstract class RegExpFactory
	{

	  /// <summary>
	  /// Creates a regular expression object from the given regular expression string.
	  /// </summary>
	  /// <param name="regExpString">
	  ///          a regular expression string </param>
	  /// <returns> a regular expression build from the regular expression string </returns>
	  /// <exception cref="InitializationException">
	  ///              if regular expression is not well formed </exception>
	  public abstract RegExp createRegExp(string regExpString);
	}

}