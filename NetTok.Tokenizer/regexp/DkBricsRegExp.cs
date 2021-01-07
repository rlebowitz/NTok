using System.Collections.Generic;
using NetTok.Tokenizer.Exceptions;

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

	using InitializationException = InitializationException;
	using AutomatonMatcher = dk.brics.automaton.AutomatonMatcher;
	using RunAutomaton = dk.brics.automaton.RunAutomaton;

	/// <summary>
	/// Implements the <seealso cref="RegExp"/> interface for regular expressions of the dk.brics.automaton
	/// package.
	/// 
	/// @author Joerg Steffen, DFKI
	/// </summary>
	public class DkBricsRegExp : RegExp
	{

	  // instance of a regular expression in the dk.brics.automaton package
	  private RunAutomaton re;


	  /// <summary>
	  /// Creates a new instance of <seealso cref="DkBricsRegExp"/> for the given regular expression string.
	  /// </summary>
	  /// <param name="regExpString">
	  ///          a regular expression string </param>
	  /// <exception cref="InitializationException">
	  ///              if regular expression is not well formed </exception>
	  public DkBricsRegExp(string regExpString)
	  {

		this.re = new RunAutomaton((new dk.brics.automaton.RegExp(regExpString)).toAutomaton(true), true);
	  }


	  /// <summary>
	  /// {@inheritDoc}
	  /// </summary>
	  public virtual IList<Match> getAllMatches(string input)
	  {

		// create AutomatonMatcher for input
		AutomatonMatcher matcher = this.re.newMatcher(input);
		// convert matches and collect them in a list
		IList<Match> matches = new List<Match>();
		while (matcher.find())
		{
		  matches.Add(new Match(matcher.start(), matcher.end(), matcher.group()));
		}
		// return result
		return matches;
	  }


	  /// <summary>
	  /// {@inheritDoc}
	  /// </summary>
	  public virtual bool matches(string input)
	  {

		// create AutomatonMatcher for input
		AutomatonMatcher matcher = this.re.newMatcher(input);
		if (matcher.find())
		{
		  return matcher.start() == 0 && matcher.end() == input.Length;
		}
		return false;
	  }


	  /// <summary>
	  /// {@inheritDoc}
	  /// </summary>
	  public virtual Match contains(string input)
	  {

		// create AutomatonMatcher for input
		AutomatonMatcher matcher = this.re.newMatcher(input);
		if (matcher.find())
		{
		  return new Match(matcher.start(), matcher.end(), matcher.group());
		}

		return null;
	  }


	  /// <summary>
	  /// {@inheritDoc}
	  /// </summary>
	  public virtual Match starts(string input)
	  {

		// create AutomatonMatcher for input
		AutomatonMatcher matcher = this.re.newMatcher(input);
		if (matcher.find() && matcher.start() == 0)
		{
		  return new Match(matcher.start(), matcher.end(), matcher.group());
		}

		return null;
	  }


	  /// <summary>
	  /// {@inheritDoc}
	  /// </summary>
	  public virtual Match ends(string input)
	  {

		// create AutomatonMatcher for input
		AutomatonMatcher matcher = this.re.newMatcher(input);
		// get the last match
		Match match = null;
		while (matcher.find())
		{
		  match = new Match(matcher.start(), matcher.end(), matcher.group());
		}
		// return result
		if (match != null && match.EndIndex == input.Length)
		{
		  return match;
		}
		return null;
	  }
	}

}