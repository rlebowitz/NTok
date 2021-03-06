﻿/*
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
    /// Extends <seealso cref="RegExpFactory"/> for regular expressions of the dk.brics.automaton package.
    /// 
    /// @author Joerg Steffen, DFKI
    /// </summary>
    public class DkBricsRegExpFactory : RegExpFactory
	{

	  /// <summary>
	  /// Creates a new instance of <seealso cref="DkBricsRegExpFactory"/>.
	  /// </summary>
	  public DkBricsRegExpFactory()
	  {

		// nothing to do
	  }


	  /// <summary>
	  /// {@inheritDoc}
	  /// </summary>
	  public override IRegExp createRegExp(string regExpString)
	  {

		return new DkBricsRegExp(regExpString);
	  }
	}

}