using System.Collections.Generic;
using System.Text;

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

namespace NetTok.Tokenizer.output
{

	/// <summary>
	/// Represents a text unit with its tokens.
	/// 
	/// @author Joerg Steffen, DFKI
	/// </summary>
	public class TextUnit
	{

	  // start index of the text unit
	  private int startIndex;

	  // end index of the text unit
	  private int endIndex;

	  // list with the tokens of the text unit
	  private IList<Token> tokens;


	  /// <summary>
	  /// Creates a new instance of <seealso cref="TextUnit"/>.
	  /// </summary>
	  public TextUnit()
	  {

		this.StartIndex = 0;
		this.EndIndex = 0;
		this.Tokens = new List<Token>();
	  }


	  /// <summary>
	  /// Creates a new instance of <seealso cref="TextUnit"/> containing the given tokens.
	  /// </summary>
	  /// <param name="tokens">
	  ///          a list of tokens </param>
	  public TextUnit(IList<Token> tokens)
	  {

		this.Tokens = tokens;
	  }


	  /// <returns> the start index </returns>
	  public virtual int StartIndex
	  {
		  get
		  {
    
			return this.startIndex;
		  }
		  set
		  {
    
			this.startIndex = value;
		  }
	  }




	  /// <returns> the end index </returns>
	  public virtual int EndIndex
	  {
		  get
		  {
    
			return this.endIndex;
		  }
		  set
		  {
    
			this.endIndex = value;
		  }
	  }




	  /// <returns> the token list </returns>
	  public virtual IList<Token> Tokens
	  {
		  get
		  {
    
			return this.tokens;
		  }
		  set
		  {
    
			this.tokens = value;
			if (value.Count > 0)
			{
			  this.StartIndex = value[0].StartIndex;
			  this.EndIndex = value[value.Count - 1].EndIndex;
			}
			else
			{
			  this.StartIndex = 0;
			  this.EndIndex = 0;
			}
		  }
	  }




	  /// <summary>
	  /// {@inheritDoc}
	  /// </summary>
	  public override string ToString()
	  {

//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
		StringBuilder result = new StringBuilder(string.Format("  Text Unit Start: %d%n  Text Unit End: %d%n", this.StartIndex, this.EndIndex));

		// add tokens
		foreach (Token oneToken in this.Tokens)
		{
		  result.Append(oneToken.ToString());
		}

		return result.ToString();
	  }
	}

}