using System.Collections.Generic;
using System.Text;
using NetTok.Tokenizer.exceptions;

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

	using ProcessingException = ProcessingException;

	/// <summary>
	/// <seealso cref="FastAnnotatedString"/> is a fast implementation of the <seealso cref="AnnotatedString"/> interface. It
	/// reserves an array of objects and an array of booleans for each newly introduced annotation key.
	/// This provides fast access at the cost of memory. So only introduce new annotation keys if
	/// necessary.
	/// 
	/// @author Joerg Steffen, DFKI
	/// </summary>
	public class FastAnnotatedString : AnnotatedString
	{

	  // current index within the string
	  private int index;

	  // index position at the end of the string
	  private int endIndex;

	  // content of the string as a character array
	  private char[] content;

	  // map of annotation keys to arrays of objects holding the annotation values;
	  // the object at a certain index in the array is the annotation value of the corresponding
	  // character in the annotated string
	  private IDictionary<string, object> annotations;

	  // map of annotation keys to arrays of booleans holding annotation borders
	  private IDictionary<string, bool[]> borders;

	  // last annotation key used
	  private string currentKey;

	  // last value array used
	  private object[] currentValues;

	  // last border array used
	  private bool[] currentBorders;


	  /// <summary>
	  /// Creates a new instance of <seealso cref="FastAnnotatedString"/> for the given input text.
	  /// </summary>
	  /// <param name="inputText">
	  ///          the text to annotate </param>
	  public FastAnnotatedString(string inputText)
	  {

		// check if there is a string
		if (string.ReferenceEquals(inputText, null))
		{
		  throw new System.NullReferenceException("null as input string is not allowed");
		}
		// initialization
		this.endIndex = inputText.Length;
		this.content = inputText.ToCharArray();
		this.annotations = new Dictionary<string, object>(5);
		this.borders = new Dictionary<string, bool[]>(5);
		this.currentKey = null;
		this.currentBorders = null;
		this.currentValues = null;
		this.index = 0;
	  }


	  /// <summary>
	  /// {@inheritDoc}
	  /// </summary>
	  public override char first()
	  {

		this.index = 0;
		return current();
	  }


	  /// <summary>
	  /// {@inheritDoc}
	  /// </summary>
	  public override char last()
	  {

		if (0 != this.endIndex)
		{
		  this.index = this.endIndex - 1;
		}
		else
		{
		  this.index = this.endIndex;
		}
		return current();
	  }


	  /// <summary>
	  /// {@inheritDoc}
	  /// </summary>
	  public override char current()
	  {

		if ((this.index >= 0) && (this.index < this.endIndex))
		{
		  return this.content[this.index];
		}
		return DONE;
	  }


	  /// <summary>
	  /// {@inheritDoc}
	  /// </summary>
	  public override char next()
	  {

		if (this.index < (this.endIndex - 1))
		{
		  this.index++;
		  return this.content[this.index];
		}
		this.index = this.endIndex;
		return DONE;
	  }


	  /// <summary>
	  /// {@inheritDoc}
	  /// </summary>
	  public override char previous()
	  {

		if (this.index > 0)
		{
		  this.index--;
		  return this.content[this.index];
		}
		return DONE;
	  }


	  /// <summary>
	  /// {@inheritDoc}
	  /// </summary>
	  public override int BeginIndex
	  {
		  get
		  {
    
			return 0;
		  }
	  }


	  /// <summary>
	  /// {@inheritDoc}
	  /// </summary>
	  public override int EndIndex
	  {
		  get
		  {
    
			return this.endIndex;
		  }
	  }


	  /// <summary>
	  /// {@inheritDoc}
	  /// </summary>
	  public override int Index
	  {
		  get
		  {
    
			return this.index;
		  }
	  }


	  /// <summary>
	  /// {@inheritDoc}
	  /// </summary>
	  public override char setIndex(int position)
	  {

		if ((position < 0) || (position > this.endIndex))
		{
		  throw new System.ArgumentException(string.Format("Invalid index {0:D}", position));
		}
		this.index = position;
		return current();
	  }


	  /// <summary>
	  /// {@inheritDoc}
	  /// </summary>
	  public override object clone()
	  {

		try
		{
		  FastAnnotatedString other = (FastAnnotatedString)base.clone();
		  return other;
		}
		catch (CloneNotSupportedException cnse)
		{
		  throw new ProcessingException(cnse.LocalizedMessage, cnse);
		}
	  }


	  /// <summary>
	  /// {@inheritDoc}
	  /// </summary>
	  public virtual char charAt(int charIndex)
	  {

		if ((charIndex < 0) || (charIndex > this.endIndex))
		{
		  throw new System.ArgumentException(string.Format("Invalid index {0:D}", charIndex));
		}
		if ((charIndex >= 0) && (charIndex < this.endIndex))
		{
		  return this.content[charIndex];
		}
		return DONE;
	  }


	  /// <summary>
	  /// {@inheritDoc}
	  /// </summary>
	  public virtual string substring(int start, int end)
	  {

		if ((start < 0) || (end > this.endIndex) || (start > end))
		{
		  throw new System.ArgumentException(string.Format("Invalid substring range {0:D} - {1:D}", start, end));
		}
		return new string(this.content, start, end - start);
	  }


	  /// <summary>
	  /// {@inheritDoc}
	  /// </summary>
	  public virtual void annotate(string key, object value, int start, int end)
	  {

		// check if range is legal
		if ((start < 0) || (end > this.endIndex) || (start >= end))
		{
		  throw new System.ArgumentException(string.Format("Invalid substring range {0:D} - {1:D}", start, end));
		}

		if (!key.Equals(this.currentKey))
		{
		  // update currents
		  object probe = this.annotations[key];
		  if (null == probe)
		  {
			// create new arrays for this key
			this.currentValues = new object[this.endIndex];
			this.currentBorders = new bool[this.endIndex];
			this.currentKey = key;
			// if string is not empty, the first character is already a border
			if (this.endIndex > 0)
			{
			  this.currentBorders[0] = true;
			}
			// store arrays
			this.annotations[key] = this.currentValues;
			this.borders[key] = this.currentBorders;
		  }
		  else
		  {
			this.currentValues = (object[])probe;
			this.currentBorders = this.borders[key];
			this.currentKey = key;
		  }
		}

		// annotate
		for (int i = start; i < end; i++)
		{
		  this.currentValues[i] = value;
		  this.currentBorders[i] = false;
		}
		// set border for current annotation and the implicit next annotation (if there is one)
		this.currentBorders[start] = true;
		if (end < this.endIndex)
		{
		  this.currentBorders[end] = true;
		}
	  }


	  /// <summary>
	  /// {@inheritDoc}
	  /// </summary>
	  public virtual object getAnnotation(string key)
	  {

		if ((this.index >= 0) && (this.index < this.endIndex))
		{
		  if (!key.Equals(this.currentKey))
		  {
			// update currents
			object probe = this.annotations[key];
			if (null != probe)
			{
			  this.currentKey = key;
			  this.currentValues = (object[])probe;
			  this.currentBorders = this.borders[key];
			}
			else
			{
			  return null;
			}
		  }

		  // get annotation value
		  return this.currentValues[this.index];
		}

		return null;
	  }


	  /// <summary>
	  /// {@inheritDoc}
	  /// </summary>
	  public virtual int getRunStart(string key)
	  {

		if (!key.Equals(this.currentKey))
		{
		  // update currents
		  object probe = this.borders[key];
		  if (null != probe)
		  {
			this.currentKey = key;
			this.currentValues = (object[])this.annotations[key];
			this.currentBorders = (bool[])probe;
		  }
		  else
		  {
			return 0;
		  }
		}
		// search border
		for (int i = this.index; i >= 0; i--)
		{
		  if (this.currentBorders[i])
		  {
			return i;
		  }
		}
		return 0;
	  }


	  /// <summary>
	  /// {@inheritDoc}
	  /// </summary>
	  public virtual int getRunLimit(string key)
	  {

		if (!key.Equals(this.currentKey))
		{
		  // update currents
		  object probe = this.borders[key];
		  if (null != probe)
		  {
			this.currentKey = key;
			this.currentValues = (object[])this.annotations[key];
			this.currentBorders = (bool[])probe;
		  }
		  else
		  {
			return this.endIndex;
		  }
		}
		// search border
		for (int i = this.index + 1; i < this.endIndex; i++)
		{
		  if (this.currentBorders[i])
		  {
			return i;
		  }
		}
		return this.endIndex;
	  }


	  /// <summary>
	  /// {@inheritDoc}
	  /// </summary>
	  public virtual int findNextAnnotation(string key)
	  {

		if (!key.Equals(this.currentKey))
		{
		  // update currents
		  object probe = this.annotations[key];
		  if (null != probe)
		  {
			this.currentKey = key;
			this.currentValues = (object[])probe;
			this.currentBorders = this.borders[key];
		  }
		  else
		  {
			return this.endIndex;
		  }
		}

		// search next annotation
		int i;
		for (i = this.index + 1; i < this.endIndex; i++)
		{
		  if (this.currentBorders[i])
		  {
			for (int j = i; j < this.endIndex; j++)
			{
			  if (null != this.currentValues[j])
			  {
				return j;
			  }
			}
			return this.endIndex;
		  }
		}
		return this.endIndex;
	  }


	  /// <summary>
	  /// {@inheritDoc}
	  /// </summary>
	  public virtual string toString(string key)
	  {

		// init result
		StringBuilder result = new StringBuilder();
		// make a backup of the current index
		int bakupIndex = this.index;
		// iterate over string
		this.index = 0;
		while (this.index < this.endIndex)
		{
		  int endAnno = this.getRunLimit(key);
		  if (null != getAnnotation(key))
		  {
			result.Append(substring(this.index, endAnno) + "\t" + this.index + "-" + endAnno + "\t" + getAnnotation(key) + System.getProperty("line.separator"));
		  }
		  this.index = endAnno;
		}
		// restore index
		this.index = bakupIndex;
		// return result
		return result.ToString();
	  }


	  /// <summary>
	  /// {@inheritDoc}
	  /// </summary>
	  public override string ToString()
	  {

		return new string(this.content);
	  }
	}

}