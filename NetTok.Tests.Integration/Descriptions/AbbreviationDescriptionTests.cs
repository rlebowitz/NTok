using System.Collections.Generic;
using NetTok.Tokenizer.Descriptions;
using Xunit;
using Xunit.Abstractions;

namespace NetTok.Tests.Integration.Descriptions
{
    public class AbbreviationDescriptionTests
    {
        public AbbreviationDescriptionTests(ITestOutputHelper output)
        {
            Output = output;
        }

        private AbbreviationDescription Description { get; set; }
        private ITestOutputHelper Output { get; }

        [Fact]
        public void LoadEnglishAbbreviationTest()
        {
            var macrosMap = new Dictionary<string, string>();
            // the abbreviation Description references some macros that need to be preloaded.
            var macroDescription = new MacroDescription("en");
            macroDescription.Load(macrosMap);
            Description = new AbbreviationDescription("en");
            Description.Load(macrosMap);
            Assert.NotNull(macrosMap);
            Assert.Equal(3, macrosMap.Keys.Count);
            Assert.Equal(1, Description.DefinitionsMap.Keys.Count); // B_ABBREVIATION
            Assert.Equal(1, Description.RulesMap.Count); // ALL_RULE
        }

        [Fact]
        public void LoadDefaultAbbreviationTest()
        {
            var macrosMap = new Dictionary<string, string>();
            // the abbreviation Description references some macros that need to be preloaded.
            var macroDescription = new MacroDescription("default");
            macroDescription.Load(macrosMap);
            Description = new AbbreviationDescription("default");
            Description.Load(macrosMap);
            Assert.NotNull(macrosMap);
            Assert.Equal(3, macrosMap.Keys.Count);
            Assert.Equal(1, Description.DefinitionsMap.Keys.Count); // B_ABBREVIATION
            Assert.Equal(1, Description.RulesMap.Count); // ALL_RULE
        }
    }
}