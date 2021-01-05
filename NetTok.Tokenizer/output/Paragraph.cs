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
	/// Represents a paragraph with its text units.
	/// 
	/// @author Joerg Steffen, DFKI
	/// </summary>
	public class Paragraph
	{

	  // start index of the paragraph
	  private int startIndex;

	  // the end index of the paragraph
	  private int endIndex;

	  // list with the text units of the paragraph
	  private IList<TextUnit> textUnits;


	  /// <summary>
	  /// Creates a new instance of <seealso cref="Paragraph"/>.
	  /// </summary>
	  public Paragraph()
	  {

		this.StartIndex = 0;
		this.EndIndex = 0;
		this.TextUnits = new List<TextUnit>();
	  }


	  /// <summary>
	  /// Creates a new instance of <seealso cref="Paragraph"/> that contains the given text units.
	  /// </summary>
	  /// <param name="textUnits">
	  ///          a list of text units </param>
	  public Paragraph(IList<TextUnit> textUnits)
	  {

		this.TextUnits = textUnits;
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




	  /// <returns> the list with the text units </returns>
	  public virtual IList<TextUnit> TextUnits
	  {
		  get
		  {
    
			return this.textUnits;
		  }
		  set
		  {
    
			this.textUnits = value;
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
		StringBuilder result = new StringBuilder(string.Format("Paragraph Start: %d%nParagraph End: %d%n", this.StartIndex, this.EndIndex));

		// add text units
		foreach (TextUnit oneTu in this.TextUnits)
		{
		  result.Append(oneTu.ToString());
		}

		return result.ToString();
	  }
	}

}