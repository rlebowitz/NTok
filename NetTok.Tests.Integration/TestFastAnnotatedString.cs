using System.IO;
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

namespace de.dfki.lt.tools.tokenizer.annotate
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.nullValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;


	using Test = org.junit.Test;

	/// <summary>
	/// Test class for <seealso cref="FastAnnotatedString"/>.
	/// 
	/// @author Joerg Steffen, DFKI
	/// </summary>
	public class TestFastAnnotatedString
	{

	  /// <summary>
	  /// Tests annotated Strings.
	  /// </summary>
	  /// <exception cref="IOException">
	  ///           if there is an error when reading the result file </exception>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testFastAnnotatedString() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
	  public virtual void testFastAnnotatedString()
	  {

		AnnotatedString input1 = new FastAnnotatedString("This is a test.");
		// 0123456789012345
		input1.annotate("type", "tok", 0, 4);
		input1.annotate("type", "tok", 5, 7);
		input1.annotate("type", "tok", 8, 9);
		input1.annotate("type", "tok", 10, 14);
		input1.annotate("type", "punct", 14, 15);
		compareResults(input1, "expected-results/annotated-string-expected-1.txt");

		AnnotatedString input2 = new FastAnnotatedString("sdfslkdflsdfsldfksdf");
		input2.annotate("type", "tok", 5, 15);
		assertThat(input2.toString("type").Trim(), @is("kdflsdfsld\t5-15\ttok"));

		input2.annotate("type", "mid", 9, 12);
		compareResults(input2, "expected-results/annotated-string-expected-2.txt");
	  }


	  /// <summary>
	  /// Compares the string representation of the given annotation and the expected result as read from
	  /// the given file name.
	  /// </summary>
	  /// <param name="input">
	  ///          the annotated string </param>
	  /// <param name="resFileName">
	  ///          the result file name </param>
	  /// <exception cref="IOException">
	  ///           if there is an error when reading the result file </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void compareResults(AnnotatedString input, String resFileName) throws java.io.IOException
	  private void compareResults(AnnotatedString input, string resFileName)
	  {

		StreamReader resReader = new StreamReader(this.GetType().ClassLoader.getResourceAsStream(resFileName), Encoding.UTF8);
		StreamReader inputReader = new StreamReader(new StringReader(input.toString("type")));
		// compare line by line with expected result
		int lineCount = 1;
		string resLine;
		while (!string.ReferenceEquals((resLine = resReader.ReadLine()), null))
		{
		  string inputLine = inputReader.ReadLine();
		  assertThat(inputLine, @is(not(nullValue())));
		  assertThat(resFileName + ": line " + lineCount, resLine, @is(inputLine));
		  lineCount++;
		}
	  }
	}
}
