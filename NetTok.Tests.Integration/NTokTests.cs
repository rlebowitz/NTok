using System;
using System.IO;
using System.Text;
using NetTok.Tokenizer;
using NetTok.Tokenizer.Output;
using Xunit;
using Xunit.Abstractions;

/*
  NTok
 * A configurable tokenizer implemented in C# based on the Java JTok tokenizer.
 *
 * (c) 2003 - 2014  DFKI Language Technology Lab http://www.dfki.de/lt
 *   Author: Joerg Steffen, steffen@dfki.de
 *
 * (c) 2021 - Finaltouch IT LLC
 *   Author:  Robert Lebowitz, lebowitz@finaltouch.com
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

namespace NetTok.Tests.Integration
{
    /// <summary>
    ///     Test class for <seealso cref="NTok" />.
    ///     @author Joerg Steffen, DFKI, Robert J Lebowitz, Finaltouch IT LLC
    /// </summary>
    public class NTokTests
    {
        public NTokTests(ITestOutputHelper output)
        {
            Tokenizer = new NTok();
            Output = output;
        }

        // the tokenizer to test
        public NTok Tokenizer { get; }

        private ITestOutputHelper Output { get; }


        /// <summary>
        ///     Tests the method <seealso cref="NTok.Tokenize(string, string)" />.
        /// </summary>
        public void TestGerman()
        {
            CompareResults("german/amazon.txt", "de", "expected-results/german/amazon-expected.txt");
            CompareResults("german/german.txt", "de", "expected-results/german/german-expected.txt");
        }


        /// <summary>
        ///     Tests the method <seealso cref="NTok.Tokenize(string, string)" />.
        /// </summary>
        public void TestEnglish()
        {
            // English
            CompareResults("english/amazon-coleman.txt", "en", "expected-results/english/amazon-coleman-expected.txt");
            CompareResults("english/english.txt", "en", "expected-results/english/english-expected.txt");
            CompareResults("english/randomhouse-hertsgaard.txt", "en",
                "expected-results/english/randomhouse-hertsgaard-expected.txt");
        }


        /// <summary>
        ///     Tests the method <seealso cref="NTok.Tokenize(string, string)" />.
        /// </summary>
        public void TestClitics()
        {
            // Other
            CompareResults("test/cliticsTest.txt", "en", "expected-results/test/cliticsTest-expected.txt");
        }


        /// <summary>
        ///     Tests the method <seealso cref="NTok.Tokenize(string, string)" />.
        /// </summary>
        public void TestMisc()
        {
            CompareResults("test/misc.txt", "en", "expected-results/test/misc-expected.txt");
        }


        /// <summary>
        ///     Tests the method <seealso cref="NTok.Tokenize(string, string)" />.
        /// </summary>
        public void TestNumbers()
        {
            CompareResults("test/numbersTest.txt", "de", "expected-results/test/numbersTest-expected.txt");
        }

        /// <summary>
        ///     Tests the method <seealso cref="NTok.Tokenize(string, string)" />.
        /// </summary>
        public void TestParagraphs()
        {
            CompareResults("test/paragraphTest.txt", "en", "expected-results/test/paragraphTest-expected.txt");
        }

        /// <summary>
        ///     Tests the method <seealso cref="NTok.Tokenize(string, string)" />.
        /// </summary>
        public virtual void TestPunctuation()
        {
            CompareResults("test/punctuationTest.txt", "en", "expected-results/test/punctuationTest-expected.txt");
        }

        /// <summary>
        ///     Tests the method <seealso cref="JTok.tokenize(string, string)" />.
        /// </summary>
        public virtual void TestSpecialCharacters()
        {
            CompareResults("test/specialCharactersTest.txt", "de",
                "expected-results/test/specialCharactersTest-expected.txt");
        }


        /// <summary>
        ///     Tests the method <seealso cref="JTok.tokenize(string, string)" />.
        /// </summary>
        public virtual void TestTextUnits()
        {
            CompareResults("test/tuTest.txt", "de", "expected-results/test/tuTest-expected.txt");
        }


        /// <summary>
        ///     Compares the tokenization result of the given input with the result as read from the given file
        ///     name.
        /// </summary>
        /// <param name="inputFileName">
        ///     the input file to tokenize
        /// </param>
        /// <param name="language">
        ///     the language of the input file
        /// </param>
        /// <param name="fileName">
        ///     the result file name
        /// </param>
        private void CompareResults(string inputFileName, string language, string fileName)
        {
            Console.WriteLine(inputFileName);
            // tokenize input file
            using var reader = new StreamReader(ResourceMethods.ReadResource(inputFileName));
            var input = new string(reader.ReadToEnd());
            var result = new StringBuilder();
            // print result as paragraphs with text units and tokens
            foreach (var onePara in Outputter.CreateParagraphs(Tokenizer.Tokenize(input, language)))
            {
                result.AppendLine(onePara.ToString());
            }

            // compare line by line with expected result
            using var inputReader = new StreamReader(ResourceMethods.ReadResource(inputFileName));
            using var resourceReader = new StreamReader(ResourceMethods.ReadResource(fileName));
            var lineCount = 1;
            string resLine;
            while ((resLine = resourceReader.ReadLine()) != null)
            {
                var inputLine = inputReader.ReadLine();
                Assert.NotNull(inputLine);
                if (!inputLine.Equals(resLine))
                {
                    Output.WriteLine(
                        $"File: {fileName}\t Line: {lineCount}\t Input: {inputLine}\t Resource: {resLine}");
                }

                Assert.Equal(resLine, inputLine);
                lineCount++;
            }
        }
    }
}