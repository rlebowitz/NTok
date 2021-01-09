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

namespace NetTok.Tokenizer.Output
{
    /// <summary>
	/// Represents a token with its type and surface image.
	/// 
	/// @author Joerg Steffen, DFKI
	/// </summary>
	public class Token
	{

	  // the Penn Treebank replacements for brackets:
	  private const string LRB = "-LRB-";
	  private const string RRB = "-RRB-";
	  private const string LSB = "-LSB-";
	  private const string RSB = "-RSB-";
	  private const string LCB = "-LCB-";
	  private const string RCB = "-RCB-";

	  // start index of the token
	  private int startIndex;

	  // end index of the token
	  private int endIndex;

	  // type of the token
	  private string type;

	  // surface image of the token
	  private string image;


	  /// <summary>
	  /// Creates a new instance of <seealso cref="Token"/>.
	  /// </summary>
	  public Token()
	  {

		this.StartIndex = 0;
		this.EndIndex = 0;
		this.Type = "";
		this.Image = "";
	  }


	  /// <summary>
	  /// Creates a new instance of <seealso cref="Token"/> for the given start index, end index, type and surface
	  /// image.
	  /// </summary>
	  /// <param name="startIndex">
	  ///          the start index </param>
	  /// <param name="endIndex">
	  ///          the end index </param>
	  /// <param name="type">
	  ///          the type </param>
	  /// <param name="image">
	  ///          the surface image </param>
	  public Token(int startIndex, int endIndex, string type, string image)
	  {

		this.StartIndex = startIndex;
		this.EndIndex = endIndex;
		this.Type = type;
		this.Image = image;
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




	  /// <returns> the token type </returns>
	  public virtual string Type
	  {
		  get
		  {
    
			return this.type;
		  }
		  set
		  {
    
			this.type = value;
		  }
	  }




	  /// <returns> the surface image </returns>
	  public virtual string Image
	  {
		  get
		  {
    
			return this.image;
		  }
		  set
		  {
    
			this.image = value;
		  }
	  }




	  /// <summary>
	  /// Returns the Penn Treebank surface image of the token if a Penn Treebank replacement took place,
	  /// {@code null} otherwise.
	  /// </summary>
	  /// <returns> the surface image as the result of the Penn Treebank token replacement or {@code null} </returns>
	  public virtual string PtbImage
	  {
		  get
		  {
    
			return applyPtbFormat(this.image, this.type);
		  }
	  }


	  /// <summary>
	  /// {@inheritDoc}
	  /// </summary>
	  public override string ToString()
	  {

		StringBuilder result = new StringBuilder(string.Format("    Token: {0,-15}\tType: {1}\tStart: {2}\tEnd: {3}", string.Format("\"{0}\"", this.Image), this.Type, this.StartIndex, this.EndIndex));

		string ptbImage = applyPtbFormat(this.image, this.type);
		if (null != ptbImage)
		{
		  result.Append(string.Format("\tPTB: \"{0}\"", ptbImage));
		}
		result.Append(string.Format("%n"));

		return result.ToString();
	  }


	  /// <summary>
	  /// This applies some replacements used in the Penn Treebank format to the given token image of the
	  /// given type.
	  /// </summary>
	  /// <param name="image">
	  ///          the token image </param>
	  /// <param name="type">
	  ///          the type </param>
	  /// <returns> a modified string or {@code null} if no replacement took place </returns>
	  public static string applyPtbFormat(string image, string type)
	  {

		string result = null;

		if (type.Equals(PunctuationDescription.OpenBracket))
		{

		  if (image.Equals("("))
		  {
			result = LRB;
		  }
		  else if (image.Equals("["))
		  {
			result = LSB;
		  }
		  else if (image.Equals("{"))
		  {
			result = LCB;
		  }
		}
		else if (type.Equals(PunctuationDescription.CloseBracket))
		{

		  if (image.Equals(")"))
		  {
			result = RRB;
		  }
		  else if (image.Equals("]"))
		  {
			result = RSB;
		  }
		  else if (image.Equals("}"))
		  {
			result = RCB;
		  }
		}
		else if (type.Equals(PunctuationDescription.OpenPunct))
		{
		  result = "``";
		}
		else if (type.Equals(PunctuationDescription.ClosePunct))
		{
		  result = "''";
		}
		else if (image.Contains("/"))
		{
		  result = image.Replace("/", "\\/");
		}
		else if (image.Contains("*"))
		{
		  result = image.Replace("*", "\\*");
		}

		return result;
	  }
	}

}