using System;
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

namespace de.dfki.lt.tools.tokenizer
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.nullValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;


	using BeforeClass = org.junit.BeforeClass;
	using Test = org.junit.Test;

	using Outputter = de.dfki.lt.tools.tokenizer.output.Outputter;
	using Paragraph = de.dfki.lt.tools.tokenizer.output.Paragraph;

	/// <summary>
	/// Test class for <seealso cref="JTok"/>.
	/// 
	/// @author Joerg Steffen, DFKI
	/// </summary>
	public class TestJTok
	{

	  // the tokenizer to test
	  private static JTok tokenizer;


	  /// <summary>
	  /// Initializes the tokenizer.
	  /// </summary>
	  /// <exception cref="IOException">
	  ///           if there is an error during initialization </exception>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void oneTimeSetUp() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
	  public static void oneTimeSetUp()
	  {

		Properties tokProps = new Properties();
		Stream @in = FileTools.openResourceFileAsStream(Paths.get("jtok/jtok.cfg"));
		tokProps.load(@in);
		@in.Close();
		// create new instance of JTok
		tokenizer = new JTok(tokProps);
	  }


	  /// <summary>
	  /// Tests the method <seealso cref="JTok.tokenize(string, string)"/>.
	  /// </summary>
	  /// <exception cref="IOException">
	  ///           if there is an error when reading files </exception>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testGerman() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
	  public virtual void testGerman()
	  {

		// German
		this.compareResults("german/amazon.txt", "de", "expected-results/german/amazon-expected.txt");
		this.compareResults("german/german.txt", "de", "expected-results/german/german-expected.txt");
	  }


	  /// <summary>
	  /// Tests the method <seealso cref="JTok.tokenize(string, string)"/>.
	  /// </summary>
	  /// <exception cref="IOException">
	  ///           if there is an error when reading files </exception>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testEnglish() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
	  public virtual void testEnglish()
	  {

		// English
		this.compareResults("english/amazon-coleman.txt", "en", "expected-results/english/amazon-coleman-expected.txt");
		this.compareResults("english/english.txt", "en", "expected-results/english/english-expected.txt");
		this.compareResults("english/randomhouse-hertsgaard.txt", "en", "expected-results/english/randomhouse-hertsgaard-expected.txt");
	  }


	  /// <summary>
	  /// Tests the method <seealso cref="JTok.tokenize(string, string)"/>.
	  /// </summary>
	  /// <exception cref="IOException">
	  ///           if there is an error when reading files </exception>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testClitics() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
	  public virtual void testClitics()
	  {

		// Other
		this.compareResults("test/cliticsTest.txt", "en", "expected-results/test/cliticsTest-expected.txt");
	  }


	  /// <summary>
	  /// Tests the method <seealso cref="JTok.tokenize(string, string)"/>.
	  /// </summary>
	  /// <exception cref="IOException">
	  ///           if there is an error when reading files </exception>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testMisc() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
	  public virtual void testMisc()
	  {

		this.compareResults("test/misc.txt", "en", "expected-results/test/misc-expected.txt");
	  }


	  /// <summary>
	  /// Tests the method <seealso cref="JTok.tokenize(string, string)"/>.
	  /// </summary>
	  /// <exception cref="IOException">
	  ///           if there is an error when reading files </exception>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNumbers() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
	  public virtual void testNumbers()
	  {

		this.compareResults("test/numbersTest.txt", "de", "expected-results/test/numbersTest-expected.txt");
	  }


	  /// <summary>
	  /// Tests the method <seealso cref="JTok.tokenize(string, string)"/>.
	  /// </summary>
	  /// <exception cref="IOException">
	  ///           if there is an error when reading files </exception>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testParagraphs() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
	  public virtual void testParagraphs()
	  {

		this.compareResults("test/paragraphTest.txt", "en", "expected-results/test/paragraphTest-expected.txt");
	  }


	  /// <summary>
	  /// Tests the method <seealso cref="JTok.tokenize(string, string)"/>.
	  /// </summary>
	  /// <exception cref="IOException">
	  ///           if there is an error when reading files </exception>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testPunctuation() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
	  public virtual void testPunctuation()
	  {

		this.compareResults("test/punctuationTest.txt", "en", "expected-results/test/punctuationTest-expected.txt");
	  }


	  /// <summary>
	  /// Tests the method <seealso cref="JTok.tokenize(string, string)"/>.
	  /// </summary>
	  /// <exception cref="IOException">
	  ///           if there is an error when reading files </exception>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testSpecialCharacters() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
	  public virtual void testSpecialCharacters()
	  {

		this.compareResults("test/specialCharactersTest.txt", "de", "expected-results/test/specialCharactersTest-expected.txt");
	  }


	  /// <summary>
	  /// Tests the method <seealso cref="JTok.tokenize(string, string)"/>.
	  /// </summary>
	  /// <exception cref="IOException">
	  ///           if there is an error when reading files </exception>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testTextUnits() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
	  public virtual void testTextUnits()
	  {

		this.compareResults("test/tuTest.txt", "de", "expected-results/test/tuTest-expected.txt");
	  }


	  /// <summary>
	  /// Compares the tokenization result of the given input with the result as read from the given file
	  /// name.
	  /// </summary>
	  /// <param name="inputFileName">
	  ///          the input file to tokenize </param>
	  /// <param name="lang">
	  ///          the language of the input file </param>
	  /// <param name="resFileName">
	  ///          the result file name </param>
	  /// <exception cref="IOException">
	  ///           if there is an error when reading the result file </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void compareResults(String inputFileName, String lang, String resFileName) throws java.io.IOException
	  private void compareResults(string inputFileName, string lang, string resFileName)
	  {

		Console.WriteLine(inputFileName);
		// tokenize input file
		Stream @in = this.GetType().ClassLoader.getResourceAsStream(inputFileName);
		string input = new string(FileTools.readInputStreamToByteArray(@in), "utf-8");
		StringBuilder result = new StringBuilder();
		string newline = System.getProperty("line.separator");
		// print result as paragraphs with text units and tokens
		foreach (Paragraph onePara in Outputter.createParagraphs(tokenizer.tokenize(input, lang)))
		{
		  result.Append(onePara.ToString());
		  result.Append(newline);
		}

		// compare line by line with expected result
		StreamReader resReader = new StreamReader(this.GetType().ClassLoader.getResourceAsStream(resFileName), Encoding.UTF8);
		StreamReader inputReader = new StreamReader(new StringReader(result.ToString()));
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
