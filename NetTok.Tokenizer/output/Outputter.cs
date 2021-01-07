using System.Collections.Generic;
using NetTok.Tokenizer.annotate;

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
	/// <seealso cref="Outputter"/> provides static methods that convert an <seealso cref="IAnnotatedString"/> into a list of
	/// nested representation of <seealso cref="Paragraph"/>s with <seealso cref="TextUnit"/>s and <seealso cref="Token"/>s.
	/// 
	/// @author Joerg Steffen, DFKI
	/// </summary>
	public sealed class Outputter
	{

	  // would create a new instance of {@link Outputter}; not to be used
	  private Outputter()
	  {

		// private constructor to enforce noninstantiability
	  }


	  /// <summary>
	  /// Creates a list of <seealso cref="Paragraph"/>s with <seealso cref="TextUnit"/>s and <seealso cref="Token"/>s from the given
	  /// annotated string.
	  /// </summary>
	  /// <param name="input">
	  ///          the annotated string </param>
	  /// <returns> a list of paragraphs </returns>
	  public static IList<Paragraph> createParagraphs(IAnnotatedString input)
	  {

		// init lists for paragraphs, text units and tokens
		IList<Paragraph> paraList = new List<Paragraph>();
		IList<TextUnit> tuList = new List<TextUnit>();
		IList<Token> tokenList = new List<Token>();

		// iterate over tokens and create token instances
		char c = input.setIndex(0);
		while (c != CharacterIterator.DONE)
		{

		  int tokenStart = input.GetRunStart(NTok.ClassAnnotation);
		  int tokenEnd = input.GetRunLimit(NTok.ClassAnnotation);
		  // check if c belongs to a token
		  string type = (string)input.GetAnnotation(NTok.ClassAnnotation);
		  if (null != type)
		  {
			// create new token instance
			Token tok = new Token(tokenStart, tokenEnd, type, input.Substring(tokenStart, tokenEnd - tokenStart));

			// check if token is first token of a paragraph or text unit
			if (null != input.GetAnnotation(NTok.BorderAnnotation))
			{
			  // add current text unit to paragraph and create new one
			  tuList.Add(new TextUnit(tokenList));
			  tokenList = new List<Token>();
			}

			// check if token is first token of a paragraph
			if (input.GetAnnotation(NTok.BorderAnnotation) == NTok.PBorder)
			{
			  // add current paragraph to result list and create new one
			  paraList.Add(new Paragraph(tuList));
			  tuList = new List<TextUnit>();
			}

			// add token to token list
			tokenList.Add(tok);
		  }
		  // set iterator to next token
		  c = input.setIndex(tokenEnd);
		}
		// add last text unit
		tuList.Add(new TextUnit(tokenList));

		// add last paragraph
		paraList.Add(new Paragraph(tuList));

		// return paragraph list
		return paraList;
	  }


	  /// <summary>
	  /// Creates a list of <seealso cref="Token"/>s from the given annotated string. Text units and paragraphs are
	  /// ignored.
	  /// </summary>
	  /// <param name="input">
	  ///          the annotated string </param>
	  /// <returns> a list of tokens </returns>
	  public static IList<Token> createTokens(IAnnotatedString input)
	  {

		// init list for tokens
		IList<Token> tokenList = new List<Token>();

		// iterate over tokens and create token instances
		char c = input.setIndex(0);
		while (c != CharacterIterator.DONE)
		{

		  int tokenStart = input.GetRunStart(NTok.ClassAnnotation);
		  int tokenEnd = input.GetRunLimit(NTok.ClassAnnotation);
		  // check if c belongs to a token
		  string type = (string)input.GetAnnotation(NTok.ClassAnnotation);
		  if (null != type)
		  {
			// create new token instance
			Token tok = new Token(tokenStart, tokenEnd, type, input.Substring(tokenStart, tokenEnd - tokenStart));

			// add token to token list
			tokenList.Add(tok);
		  }
		  // set iterator to next token
		  c = input.setIndex(tokenEnd);
		}

		// return paragraph list
		return tokenList;
	  }
	}

}