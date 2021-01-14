using System.Collections.Generic;
using NetTok.Tokenizer.Descriptions;
using Xunit;
using Xunit.Abstractions;

namespace NetTok.Tests.Integration.Descriptions
{
    public class CliticsDescriptionTests
    {
        public CliticsDescriptionTests(ITestOutputHelper output)
        {
            Output = output;
        }

        private CliticsDescription Description { get; set; }
        private ITestOutputHelper Output { get; }

        [Fact]
        public void LoadEnglishCliticsTest()
        {
            var macrosMap = new Dictionary<string, string>();
            Description = new CliticsDescription("en");
            Description.Load(macrosMap);
            Assert.NotNull(macrosMap);
            Assert.Empty(macrosMap.Keys);
            Assert.Equal(2, Description.DefinitionsMap.Keys.Count); 
            Assert.Equal(2, Description.RulesMap.Count); // ALL_RULE
        }

        [Fact]
        public void LoadDefaultCliticTest()
        {
            var macrosMap = new Dictionary<string, string>();
            Description = new CliticsDescription("default");
            Description.Load(macrosMap);
            Assert.NotNull(macrosMap);
            Assert.Empty(macrosMap.Keys);
            Assert.Equal(2, Description.DefinitionsMap.Keys.Count); 
            Assert.Equal(2, Description.RulesMap.Count); // ALL_RULE
        }
    }
}