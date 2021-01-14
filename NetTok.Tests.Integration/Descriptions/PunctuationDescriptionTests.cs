using System.Collections.Generic;
using NetTok.Tokenizer.Descriptions;
using Xunit;
using Xunit.Abstractions;

namespace NetTok.Tests.Integration.Descriptions
{
    public class PunctuationDescriptionTests
    {
        public PunctuationDescriptionTests(ITestOutputHelper output)
        {
            Output = output;
        }

        private PunctuationDescription Description { get; set; }
        private ITestOutputHelper Output { get; }

        [Fact]
        public void LoadEnglishPunctuationTest()
        {
            var macrosMap = new Dictionary<string, string>();
            Description = new PunctuationDescription("en");
            Description.Load(macrosMap);
            Assert.NotNull(macrosMap);
            Assert.Empty(macrosMap.Keys);
            Assert.Equal(50, Description.DefinitionsMap.Keys.Count);
            Assert.Equal(3, Description.RulesMap.Count);
        }

        [Fact]
        public void LoadDefaultPunctuationTest()
        {
            var macrosMap = new Dictionary<string, string>();
            Description = new PunctuationDescription("default");
            Description.Load(macrosMap);
            Assert.NotNull(macrosMap);
            Assert.Empty(macrosMap.Keys);
            Assert.Equal(50, Description.DefinitionsMap.Keys.Count);
            Assert.Equal(3, Description.RulesMap.Count);
        }
    }
}