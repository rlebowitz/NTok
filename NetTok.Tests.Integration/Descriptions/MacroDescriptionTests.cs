using System.Collections.Generic;
using NetTok.Tokenizer.Descriptions;
using Xunit;
using Xunit.Abstractions;

namespace NetTok.Tests.Integration.Descriptions
{
    public class MacroDescriptionTests
    {
        public MacroDescriptionTests(ITestOutputHelper output)
        {
            Output = output;
        }

        private MacroDescription Description { get; set; }
        private ITestOutputHelper Output { get; }

        [Fact]
        public void LoadEnglishMacrosTest()
        {
            var map = new Dictionary<string, string>();
            Description = new MacroDescription("en");
            Description.Load(map);
            Assert.NotNull(map);
            Assert.Equal(3, map.Keys.Count);
            Assert.Equal("[A-Z]", map["LETTER_UP"]);
            Assert.Equal("[a-z]", map["LETTER_LOW"]);
            Assert.Equal("[A-Za-z]", map["LETTER_ANY"]);
        }

        [Fact]
        public void LoadDefaultMacrosTest()
        {
            var map = new Dictionary<string, string>();
            Description = new MacroDescription("default");
            Description.Load(map);
            Assert.NotNull(map);
            Assert.Equal(3, map.Keys.Count);
            Assert.Equal("[A-Z]", map["LETTER_UP"]);
            Assert.Equal("[a-z]", map["LETTER_LOW"]);
            Assert.Equal("[A-Za-z]", map["LETTER_ANY"]);
        }
    }
}