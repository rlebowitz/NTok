using System.IO;
using NetTok.Tokenizer;
using NetTok.Tokenizer.Annotate;
using Xunit;
using Xunit.Abstractions;

/*
 * NTok
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
    ///     Test class for FastAnnotatedString.
    ///     @author Joerg Steffen, DFKI, Robert J Lebowitz, Finaltouch IT LLC
    /// </summary>
    public class TestFastAnnotatedString
    {

        private ITestOutputHelper Output { get; }
        public TestFastAnnotatedString(ITestOutputHelper output)
        {
            Output = output;
        }
        /// <summary>
        ///     Tests annotated Strings.
        /// </summary>
        [Fact]
        public void AnnotatedStringTest1()
        {
            var input1 = new FastAnnotatedString("This is a test.");
            // 0123456789012345
            input1.Annotate("type", "tok", 0, 4);
            input1.Annotate("type", "tok", 5, 7);
            input1.Annotate("type", "tok", 8, 9);
            input1.Annotate("type", "tok", 10, 14);
            input1.Annotate("type", "punct", 14, 15);
            CompareResults(input1, "annotated-string-expected-1.txt");

           
        }

        [Fact]
        public void AnnotatedStringTest2()
        {
            var input2 = new FastAnnotatedString("sdfslkdflsdfsldfksdf");
            input2.Annotate("type", "tok", 5, 15);
            Assert.Equal("kdflsdfsld\t5-15\ttok", input2.ToString("type").Trim());
            input2.Annotate("type", "mid", 9, 12);
            CompareResults(input2, "annotated-string-expected-2.txt");
        }


        /// <summary>
        ///     Compares the string representation of the given annotation and the expected result as read from
        ///     the given file name.
        /// </summary>
        /// <param name="input">
        ///     the annotated string
        /// </param>
        /// <param name="fileName">
        ///     the result file name
        /// </param>
        /// <exception cref="IOException">
        ///     if there is an error when reading the result file
        /// </exception>
        private void CompareResults(IAnnotatedString input, string fileName)
        {
            using var resReader = new StreamReader(ResourceManager.Read(fileName));
            using var inputReader = new StringReader(input.ToString("type"));
            // compare line by line with expected result
            var lineCount = 1;
            string resLine;
            while ((resLine = resReader.ReadLine()) != null)
            {
                var inputLine = inputReader.ReadLine();
                Assert.NotNull(inputLine);
                if (!inputLine.Equals(resLine))
                {
                    Output.WriteLine($"File: {fileName}\t Line: {lineCount}\t Input: {inputLine}\t Resource: {resLine}");
                }
                Assert.Equal(resLine, inputLine); 
                lineCount++;
            }
        }
    }
}