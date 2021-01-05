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
	/// Holds the result of matching an input string with a regular expression.
	/// 
	/// @author Joerg Steffen, DFKI
	/// </summary>
	public class Match
	{

	  // index within the input text where the match in its entirety began
	  private int startIndex;

	  // index within the input string where the match in its entirety ends;
	  // the return value is the next position after the end of the string
	  private int endIndex;

	  // string matching the regular expression pattern
	  private string image;


	  /// <summary>
	  /// Creates a new instance of <seealso cref="Match"/> using the given parameters.
	  /// </summary>
	  /// <param name="startIndex">
	  ///          the start index </param>
	  /// <param name="endIndex">
	  ///          the end index </param>
	  /// <param name="image">
	  ///          the match </param>
	  public Match(int startIndex, int endIndex, string image)
	  {

		this.startIndex = startIndex;
		this.endIndex = endIndex;
		this.image = image;
	  }


	  /// <returns> the index within the input text where the match in its entirety began </returns>
	  public virtual int StartIndex
	  {
		  get
		  {
    
			return this.startIndex;
		  }
	  }


	  /// <returns> the index within the input string where the match in its entirety ends; the return
	  /// value is the next position after the end of the string </returns>
	  public virtual int EndIndex
	  {
		  get
		  {
    
			return this.endIndex;
		  }
	  }


	  /// <returns> the string matching the regular expression pattern </returns>
	  public virtual string Image
	  {
		  get
		  {
    
			return this.image;
		  }
	  }


	  /// <summary>
	  /// {@inheritDoc}
	  /// </summary>
	  public override string ToString()
	  {

		return string.Format("{0:D} - {1:D}: {2}", this.startIndex, this.endIndex, this.image);
	  }
	}

}