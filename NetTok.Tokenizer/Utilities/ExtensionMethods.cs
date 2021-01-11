using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace NetTok.Tokenizer.Utilities
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Returns the first ancestral XElement with the specified XName value.
        /// </summary>
        /// <param name="element">The specified ancestral XElement.</param>
        /// <param name="xName">The specified XName value.</param>
        /// <returns>The first ancestor if it exists, otherwise null.</returns>
        public static XElement Ancestor(this XElement element, string xName)
        {
            return element.Ancestors(xName).FirstOrDefault();
        }

        public static string TagName(this XElement element)
        {
            return element.Name.LocalName;
        }

        public static int EndIndex(this Match match)
        {
            return match.Index + match.Length;
        }

        public static List<Match> GetAllMatches(this Regex regex, string input)
        {
            var matches = regex.Matches(input);
            return matches.Select(match => match).ToList();
        }

        public static Match Starts(this Regex regex, string input)
        {
            if (!regex.IsMatch(input))
            {
                return null;
            }

            var match = regex.Match(input);
            return match.Index == 0 ? match : null;
        }

        public static Match Ends(this Regex regex, string input)
        {
            if (regex.IsMatch(input))
            {
                var matches = regex.Matches(input);
                var lastMatch = matches.Last();
                if (lastMatch.Index == input.Length - lastMatch.Length)
                {
                    return lastMatch;
                }
            }

            return null;
        }
    }
}
