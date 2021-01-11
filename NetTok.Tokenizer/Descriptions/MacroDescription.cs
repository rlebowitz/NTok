using System;
using System.Collections.Generic;
using System.IO;
using NetTok.Tokenizer.Exceptions;
using NetTok.Tokenizer.Utilities;

namespace NetTok.Tokenizer.Descriptions
{
    public class MacroDescription : Description
    {
        /// <summary>
        ///     Class used to load macro definitions from language-specific configuration files.
        /// </summary>
        /// <param name="language">The specified language to use.</param>
        public MacroDescription(string language) : base(language) { }

        public override void Load(IDictionary<string, string> macroMap)
        {
            using var reader = new StreamReader(ResourceManager.Read($"{Language}_{Constants.Resources.MacrosSuffix}"));
            string line;
            while ((line = reader.ReadLine()?.Trim()) != null)
            {
                if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
                {
                    continue;
                }

                var sections = line.Split(Delimiters, StringSplitOptions.RemoveEmptyEntries);
                if (sections.Length != 2)
                {
                    throw new InitializationException(
                        $"File: {Language}_{Constants.Resources.MacrosSuffix} Line: {line} is not properly formatted as a Macros line.");
                }

                var macroName = sections[0].Trim();
                var regularExpressionString = sections[1].Trim();
                // expand possible macros
                regularExpressionString = ReplaceReferences(regularExpressionString, macroMap);
                macroMap[macroName] = regularExpressionString;
            }
        }
    }
}