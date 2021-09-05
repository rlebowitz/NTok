using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using NetTok.Tokenizer;
using NetTok.Tokenizer.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace NetTok.Tests.Integration
{
    public class LanguageResourceTests
    {
        private LanguageResource Resource { get; }

        private ITestOutputHelper Output { get; }

        public LanguageResourceTests(ITestOutputHelper output)
        {
            Output = output;
            Resource = new LanguageResource("en");
        }

        [Fact]
        public void ClassesRootNameTest()
        {
            Assert.Equal("TOKEN", Resource.ClassesRootName);
        }

        [Fact]
        public void LanguageTest()
        {
            Assert.Equal("en", Resource.Language);
        }

        [Fact]
        public void AncestorMapTest()
        {
            Assert.NotNull(Resource.AncestorsMap);
            Assert.True(Resource.AncestorsMap.Keys.Count > 0);
            var abbreviations = Resource.AncestorsMap["ABBREVIATION"];
            Assert.NotNull(abbreviations);
            Assert.True(abbreviations.Count == 0);
            var nb_abbreviations = Resource.AncestorsMap["NB_ABBREVIATION"];
            Assert.NotNull(nb_abbreviations);
            Assert.True(nb_abbreviations.Count == 1);
            var opar = Resource.AncestorsMap["OPAR"];
            Assert.NotNull(opar);
            Assert.True(opar.Count == 2);
        }

        [Fact]
        public void DefinitionMapTest()
        {
            var map = Resource.ClassesDescription.DefinitionsMap;
            foreach (var (key, value) in map)
            {
                Output.WriteLine(value.IsMatch("Books")
                    ? $"Key: {key}\t Value: {value} Is Match"
                    : $"Key: {key}\t Value: {value}");
            }
        }

        [Fact]
        public void DefinitionMapTest2()
        {
            var regex = new Regex("[A-Z]+");
            Assert.Matches(regex, "Books");
        }

    }
}
