using System.Collections.Generic;
using NetTok.Tokenizer.Descriptions;
using Xunit;
using Xunit.Abstractions;

namespace NetTok.Tests.Integration.Descriptions
{
    public class TokenClassesDescriptionTests
    {
        public TokenClassesDescriptionTests(ITestOutputHelper output)
        {
            Output = output;
        }

        private TokenClassesDescription Description { get; set; }
        private ITestOutputHelper Output { get; }

        [Fact]
        public void LoadEnglishCliticsTest()
        {
            var macrosMap = new Dictionary<string, string>();
            var macroDescription = new MacroDescription("en");
            macroDescription.Load(macrosMap);
            Description = new TokenClassesDescription("en");
            Description.Load(macrosMap);
            Assert.NotNull(macrosMap);
            Assert.Equal(3, macrosMap.Keys.Count);
            Assert.Equal(7, Description.DefinitionsMap.Keys.Count); 
            Assert.Equal(1, Description.RulesMap.Count); // ALL_RULE
        }

        [Fact]
        public void LoadDefaultCliticTest()
        {
            var macrosMap = new Dictionary<string, string>();
            var macroDescription = new MacroDescription("default");
            macroDescription.Load(macrosMap);
            Description = new TokenClassesDescription("default");
            Description.Load(macrosMap);
            Assert.NotNull(macrosMap);
            Assert.Equal(3, macrosMap.Keys.Count);
            Assert.Equal(7, Description.DefinitionsMap.Keys.Count); 
            Assert.Equal(1, Description.RulesMap.Count); // ALL_RULE
        }
    }
}