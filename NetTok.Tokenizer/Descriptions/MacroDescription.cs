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
        ///     Reads the embedded macro configuration file for the specified language and adds its data to the specified map.
        /// </summary>
        /// <param name="language">The specified language.</param>
        /// <param name="fileName">The specified embedded resource file.</param>
        /// <param name="macroMap">A map of macro names to regular expression pattern strings.</param>
        /// <returns>The populated class map.</returns>
        /// <exception cref="IOException">
        ///     if there is an error when reading the configuration
        /// </exception>
        public IDictionary<string, string> LoadMacros(string language, string fileName, Dictionary<string, string> macroMap)
        {
            Guard.NotNull(fileName);
            Guard.NotNull(macroMap);
            using var reader = new StreamReader(ResourceManager.Read(language, fileName));
            string line;
            while ((line = reader.ReadLine()?.Trim()) != null)
            {
                if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
                {
                    continue;
                }

                var sections = line.Split(Description.Delimiters, StringSplitOptions.RemoveEmptyEntries);
                if (sections.Length != 2)
                {
                    throw new InitializationException($"File: {fileName} Line: {line} is not properly formatted as a Macros line.");
                }

                var macroName = sections[0].Trim();
                var regularExpressionString = sections[1].Trim();
                // expand possible macros
                regularExpressionString = ReplaceReferences(regularExpressionString, macroMap);
                macroMap[macroName] = regularExpressionString;
            }

            return macroMap;
        }
    }
}