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
        [Fact]
        public void TestGerman()
        {
            CompareResults("amazon.txt", "de", "amazon-expected.txt");
            CompareResults("german.txt", "de", "german-expected.txt");
        }


        /// <summary>
        ///     Tests the method <seealso cref="NTok.Tokenize(string, string)" />.
        /// </summary>
        [Fact]
        public void TestEnglish()
        {
            // English
            CompareResults("amazon-coleman.txt", "en", "amazon-coleman-expected.txt");
            CompareResults("english.txt", "en", "english-expected.txt");
            CompareResults("randomhouse-hertsgaard.txt", "en", "randomhouse-hertsgaard-expected.txt");
        }


        /// <summary>
        ///     Tests the method <seealso cref="NTok.Tokenize(string, string)" />.
        /// </summary>
        [Fact]
        public void TestClitics()
        {
            // Other
            CompareResults("cliticsTest.txt", "en", "cliticsTest-expected.txt");
        }


        /// <summary>
        ///     Tests the method <seealso cref="NTok.Tokenize(string, string)" />.
        /// </summary>
        [Fact]
        public void TestMisc()
        {
            CompareResults("misc.txt", "en", "misc-expected.txt");
        }


        /// <summary>
        ///     Tests the method <seealso cref="NTok.Tokenize(string, string)" />.
        /// </summary>
        [Fact]
        public void TestNumbers()
        {
            CompareResults("numbersTest.txt", "de", "numbersTest-expected.txt");
        }

        /// <summary>
        ///     Tests the method <seealso cref="NTok.Tokenize(string, string)" />.
        /// </summary>
        [Fact]
        public void TestParagraphs()
        {
            CompareResults("paragraphTest.txt", "en", "paragraphTest-expected.txt");
        }

        /// <summary>
        ///     Tests the method <seealso cref="NTok.Tokenize(string, string)" />.
        /// </summary>
        [Fact]
        public virtual void TestPunctuation()
        {
            CompareResults("punctuationTest.txt", "en", "punctuationTest-expected.txt");
        }

        /// <summary>
        ///     Tests the method <seealso cref="NTok.Tokenize(string, string)" />.
        /// </summary>
        [Fact]
        public virtual void TestSpecialCharacters()
        {
            CompareResults("specialCharactersTest.txt", "de", "specialCharactersTest-expected.txt");
        }


        /// <summary>
        ///     Tests the method <seealso cref="NTok.Tokenize(string, string)" />.
        /// </summary>
        [Fact]
        public virtual void TestTextUnits()
        {
            CompareResults("tuTest.txt", "de", "tuTest-expected.txt");
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
            using var reader = new StreamReader(ResourceManager.Read(inputFileName));
            var input = new string(reader.ReadToEnd());
            var result = new StringBuilder();
            // print result as paragraphs with text units and tokens
            foreach (var onePara in Outputter.CreateParagraphs(Tokenizer.Tokenize(input, language)))
            {
                result.AppendLine(onePara.ToString());
            }

            // compare line by line with expected result
            using var inputReader = new StreamReader(ResourceManager.Read(inputFileName));
            using var resourceReader = new StreamReader(ResourceManager.Read(fileName));
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