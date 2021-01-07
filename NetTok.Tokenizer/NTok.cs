using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using NetTok.Tokenizer.Annotate;
using NetTok.Tokenizer.Exceptions;
using NetTok.Tokenizer.output;
using NetTok.Tokenizer.regexp;
using Match = NetTok.Tokenizer.regexp.Match;

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

namespace NetTok.Tokenizer
{
    /// <summary>
    ///     Tokenizer tool that recognizes paragraphs, sentences, tokens, punctuation, numbers,
    ///     abbreviations, etc.
    ///     @author Joerg Steffen, DFKI, Robert J Lebowitz, Finaltouch IT LLC
    /// </summary>
    /// <remarks>
    ///     https://stackoverflow.com/questions/46483019/logging-from-static-members-with-microsoft-extensions-logging
    /// </remarks>
    public class NTok
    {
        /// <summary>
        ///     annotation key for the token class
        /// </summary>
        public const string ClassAnnotation = "class";

        /// <summary>
        ///     annotation key for sentences and paragraph borders
        /// </summary>
        public const string BorderAnnotation = "border";

        /// <summary>
        ///     annotation value for text unit borders
        /// </summary>
        public const string TuBorder = "tu";

        /// <summary>
        ///     annotation value for paragraph borders
        /// </summary>
        public const string PBorder = "p";

        // identifier of the default configuration
        private const string Default = "default";
        private readonly ILogger _logger;

        /// <summary>
        ///     Creates a new instance of <seealso cref="NTok" />.
        /// </summary>
        /// <exception cref="IOException">If there is an error reading the configuration</exception>
        public NTok()
        {
            var loggerFactory = (ILoggerFactory) new LoggerFactory();
            _logger = new Logger<NTok>(loggerFactory);

            LanguageResources = new Dictionary<string, LanguageResource>();
            using var reader = new StreamReader(ResourceMethods.ReadResource("ntok.cfg"));
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
                {
                    continue;
                }

                LanguageResources[line] = new LanguageResource(line);
            }
        }

        // maps each supported language to a language resource
        public Dictionary<string, LanguageResource> LanguageResources { get; }


        /// <summary>
        ///     Returns the language resource for the given language if available.
        /// </summary>
        /// <param name="language">The specified language.</param>
        /// <returns>The language resource or the default resource if language is not supported.</returns>
        public LanguageResource GetLanguageResource(string language)
        {
            if (LanguageResources.ContainsKey(language))
            {
                return LanguageResources[language];
            }

            _logger.LogInformation($"The specified language {language} is not supported; using default resource.");
            return LanguageResources[Default];
        }


        /// <summary>
        ///     Tokenizes the given text in the given language. Returns an annotated string containing the
        ///     identified paragraphs with their text units and tokens.
        /// </summary>
        /// <remarks>This method is thread-safe.</remarks>
        /// <param name="inputText">The text to tokenize.</param>
        /// <param name="language">The language of the text.</param>
        /// <returns> an annotated string </returns>
        /// <exception cref="ProcessingException">
        ///     if input data causes an error e.g. if language is not supported
        /// </exception>
        public virtual IAnnotatedString Tokenize(string inputText, string language)
        {
            var resource = GetLanguageResource(language);
            // init attributed string for annotation
            IAnnotatedString input = new FastAnnotatedString(inputText);
            IdentifyTokens(input, resource);
            IdentifyPunctuation(input, resource);
            IdentifyAbbreviations(input, resource);

            // identify sentences and paragraphs
            IdentifyTus(input, resource);

            // return result
            return input;
        }


        /// <summary>
        ///     Identifies tokens and annotates them. Tokens are sequences of non-whitespaces.
        /// </summary>
        /// <param name="input">
        ///     an annotated string
        /// </param>
        /// <param name="resource">
        ///     the language resource to use
        /// </param>
        private void IdentifyTokens(IAnnotatedString input, LanguageResource resource)
        {
            // init token start index
            var tokenStart = 0;
            // flag for indicating if new token was found
            var tokenFound = false;

            // get classes root annotation
            var rootClass = resource.ClassesRoot.TagName();

            // iterate over input
            for (var c = input.First; c != default; c = input.Next)
            {
                if (char.IsWhiteSpace(c) || c == '\u00a0')
                {
                    if (tokenFound)
                    {
                        // annotate newly identified token
                        Annotate(input, ClassAnnotation, rootClass, tokenStart, input.Index,
                            input.SubString(tokenStart, input.Index - tokenStart), resource);
                        tokenFound = false;
                    }
                }
                else if (!tokenFound)
                {
                    // a new token starts here, after some whitespaces
                    tokenFound = true;
                    tokenStart = input.Index;
                }
            }

            // annotate last token
            if (tokenFound)
            {
                Annotate(input, ClassAnnotation, rootClass, tokenStart, input.Index,
                    input.SubString(tokenStart, input.Index - tokenStart), resource);
            }
        }


        /// <summary>
        ///     Identifies punctuations in the annotated tokens of the given annotated string
        /// </summary>
        /// <param name="input">
        ///     an annotated string
        /// </param>
        /// <param name="resource">
        ///     the language resource to use
        /// </param>
        /// <exception cref="ProcessingException">
        ///     if an error occurs
        /// </exception>
        private void IdentifyPunctuation(IAnnotatedString input, LanguageResource resource)
        {
            // get the matchers needed
            var punctuationMatcher = resource.AllPunctuationMatcher;
            var internalMatcher = resource.InternalMatcher;

            // get the class of the root element of the class hierarchy;
            // only tokens with this type are further examined
            var rootClass = resource.ClassesRoot.TagName();

            // iterate over tokens
            input.Index = 0;
            var c = input.Current;
            // move to first non-whitespace
            if (null == input.GetAnnotation(ClassAnnotation))
            {
                c = input.SetIndex(input.FindNextAnnotation(ClassAnnotation));
            }

            while (c != default)
            {
                // only check tokens
                if (null == input.GetAnnotation(ClassAnnotation))
                {
                    c = input.SetIndex(input.FindNextAnnotation(ClassAnnotation));
                    continue;
                }

                // get class of token
                var tokClass = (string) input.GetAnnotation(ClassAnnotation);
                // only check tokens with the most general class
                if (!ReferenceEquals(tokClass, rootClass))
                {
                    c = input.SetIndex(input.FindNextAnnotation(ClassAnnotation));
                    continue;
                }

                // save the next token start position;
                // required because the input index might be changed later in this method
                var nextTokenStart = input.FindNextAnnotation(ClassAnnotation);

                // split punctuation on the left and right side of the token
                SplitPunctuation(input, resource);

                // update current token annotation
                tokClass = (string) input.GetAnnotation(ClassAnnotation);
                // only check tokens with the most general class
                if (!ReferenceEquals(tokClass, rootClass))
                {
                    c = input.SetIndex(nextTokenStart);
                    continue;
                }

                // split clitics from left and right side of the token
                SplitClitics(input, resource);

                // update current token annotation
                tokClass = (string) input.GetAnnotation(ClassAnnotation);
                // only check tokens with the most general class
                if (!ReferenceEquals(tokClass, rootClass))
                {
                    c = input.SetIndex(nextTokenStart);
                    continue;
                }

                // get the start index of the token
                var tokenStart = input.Index;
                // get the end index of the token c belongs to
                var tokenEnd = input.GetRunLimit(ClassAnnotation);
                // get the token content
                var image = input.SubString(tokenStart, tokenEnd - tokenStart);

                // use the all rule to split image in parts consisting of
                // punctuation and non-punctuation
                var matches = punctuationMatcher.getAllMatches(image);
                // if there is no punctuation just continue
                if (0 == matches.Count)
                {
                    c = input.SetIndex(nextTokenStart);
                    continue;
                }

                // this is the relative start position of current token within
                // the image
                var index = 0;
                // iterator over matches
                for (var i = 0; i < matches.Count; i++)
                {
                    // get next match
                    var oneMatch = matches[i];

                    // check if we have some non-punctuation before the current
                    // punctuation
                    if (index != oneMatch.StartIndex)
                    {
                        // check for internal punctuation:
                        if (internalMatcher.matches(oneMatch.Image))
                        {
                            // punctuation is internal;
                            // check for right context
                            if (HasRightContextEnd(oneMatch, matches, image, i))
                            {
                                // token not complete yet
                                continue;
                            }
                        }

                        // we have a breaking punctuation; create token for
                        // non-punctuation before the current punctuation
                        Annotate(input, ClassAnnotation, tokClass, tokenStart + index, tokenStart + oneMatch.StartIndex,
                            image[index..oneMatch.StartIndex], resource);
                        index = oneMatch.StartIndex;
                    }

                    // punctuation is not internal:
                    // get the class of the punctuation and create token for it
                    var punctuationClass = IdentifyPunctuationClass(oneMatch, null, image, resource);
                    input.Annotate(ClassAnnotation, punctuationClass, tokenStart + index,
                        tokenStart + oneMatch.EndIndex);
                    index = oneMatch.EndIndex;
                }

                // cleanup after all matches have been processed
                if (index != image.Length)
                {
                    // create a token from rest of image
                    Annotate(input, ClassAnnotation, tokClass, tokenStart + index, tokenStart + image.Length,
                        image[index..], resource);
                }

                // set iterator to next non-whitespace token
                c = input.SetIndex(nextTokenStart);
            }
        }


        /// <summary>
        ///     Splits punctuation from the left and right side of the token if possible.
        /// </summary>
        /// <param name="input">
        ///     the annotate string
        /// </param>
        /// <param name="resource">
        ///     the language resource to use
        /// </param>
        private void SplitPunctuation(IAnnotatedString input, LanguageResource resource)
        {
            // get the matchers needed
            var punctuationMatcher = resource.AllPunctuationMatcher;

            // get the class of the root element of the class hierarchy;
            // only tokens with this type are further examined
            var rootClass = resource.ClassesRoot.TagName();

            // get the start index of the token
            var tokenStart = input.Index;
            // get the end index of the token
            var tokenEnd = input.GetRunLimit(ClassAnnotation);
            // get the token content
            var image = input.SubString(tokenStart, tokenEnd - tokenStart);
            // get current token annotation
            var tokClass = (string) input.GetAnnotation(ClassAnnotation);

            // check for punctuation at the beginning of the token
            var startMatch = punctuationMatcher.starts(image);
            while (null != startMatch)
            {
                // create token for punctuation
                var punctuationClass = IdentifyPunctuationClass(startMatch, null, image, resource);
                input.Annotate(ClassAnnotation, punctuationClass, tokenStart + startMatch.StartIndex,
                    tokenStart + startMatch.EndIndex);
                tokenStart += startMatch.EndIndex;
                image = input.SubString(tokenStart, tokenEnd - tokenStart);
                input.Index = tokenStart;
                if (image.Length > 0)
                {
                    Annotate(input, ClassAnnotation, tokClass, tokenStart, tokenEnd, image, resource);
                    tokClass = (string) input.GetAnnotation(ClassAnnotation);
                    if (tokClass != rootClass)
                    {
                        // the remaining token could be matched with a non-root class,
                        // so stop splitting punctuation
                        break;
                    }

                    startMatch = punctuationMatcher.starts(image);
                }
                else
                {
                    startMatch = null;
                }
            }

            // check for punctuation at the end of the token
            var endMatch = punctuationMatcher.ends(image);
            while (null != endMatch)
            {
                // create token for punctuation
                var punctuationClass = IdentifyPunctuationClass(endMatch, null, image, resource);
                input.Annotate(ClassAnnotation, punctuationClass, tokenStart + endMatch.StartIndex,
                    tokenStart + endMatch.EndIndex);
                tokenEnd = tokenStart + endMatch.StartIndex;
                image = input.SubString(tokenStart, tokenEnd - tokenStart);
                if (image.Length > 0)
                {
                    Annotate(input, ClassAnnotation, tokClass, tokenStart, tokenEnd, image, resource);
                    tokClass = (string) input.GetAnnotation(ClassAnnotation);
                    if (!ReferenceEquals(tokClass, rootClass))
                    {
                        // the remaining token could be matched with a non-root class,
                        // so stop splitting punctuation
                        break;
                    }

                    endMatch = punctuationMatcher.ends(image);
                }
                else
                {
                    endMatch = null;
                }
            }
        }

        /// <summary>
        ///     Splits pro- and enclitics from the left and right side of the token if possible.
        /// </summary>
        /// <param name="input">
        ///     the annotate string
        /// </param>
        /// <param name="resource">
        ///     the language resource to use
        /// </param>
        private void SplitClitics(IAnnotatedString input, LanguageResource resource)
        {
            // get matchers needed for clitics recognition
            var procliticsMatcher = resource.ProcliticsMatcher;
            var encliticsMatcher = resource.EncliticsMatcher;

            // get the class of the root element of the class hierarchy;
            // only tokens with this type are further examined
            var rootClass = resource.ClassesRoot.TagName();

            // get the start index of the token
            var tokenStart = input.Index;
            // get the end index of the token c belongs to
            var tokenEnd = input.GetRunLimit(ClassAnnotation);
            // get the token content
            var image = input.SubString(tokenStart, tokenEnd - tokenStart);
            // get current token annotation
            var tokClass = (string) input.GetAnnotation(ClassAnnotation);

            // check for proclitics
            var proclitic = procliticsMatcher.starts(image);
            // create token for proclitic
            while (null != proclitic)
            {
                var identifyClass = IdentifyClass(proclitic.Image, procliticsMatcher, resource.CliticsDescription);
                input.Annotate(ClassAnnotation, identifyClass, tokenStart + proclitic.StartIndex,
                    tokenStart + proclitic.EndIndex);
                tokenStart += proclitic.EndIndex;
                image = input.SubString(tokenStart, tokenEnd - tokenStart);
                input.Index = tokenStart;
                if (image.Length > 0)
                {
                    Annotate(input, ClassAnnotation, tokClass, tokenStart, tokenEnd, image, resource);
                    tokClass = (string) input.GetAnnotation(ClassAnnotation);
                    if (tokClass != rootClass)
                    {
                        // the remaining token could be matched with a non-root class,
                        // so stop splitting proclitics
                        break;
                    }

                    proclitic = procliticsMatcher.starts(image);
                }
                else
                {
                    proclitic = null;
                }
            }

            // check for enclitics
            var enclitic = encliticsMatcher.ends(image);
            while (null != enclitic)
            {
                // create tokens for enclitic
                var cliticClass = IdentifyClass(enclitic.Image, encliticsMatcher, resource.CliticsDescription);
                input.Annotate(ClassAnnotation, cliticClass, tokenStart + enclitic.StartIndex,
                    tokenStart + enclitic.EndIndex);
                tokenEnd = tokenStart + enclitic.StartIndex;
                image = input.SubString(tokenStart, tokenEnd - tokenStart);
                if (image.Length > 0)
                {
                    Annotate(input, ClassAnnotation, tokClass, tokenStart, tokenEnd, image, resource);
                    tokClass = (string) input.GetAnnotation(ClassAnnotation);
                    if (tokClass != rootClass)
                    {
                        // the remaining token could be matched with a non-root class,
                        // so stop splitting enclitics
                        break;
                    }

                    enclitic = encliticsMatcher.ends(image);
                }
                else
                {
                    enclitic = null;
                }
            }
        }


        /// <summary>
        ///     Returns {@code true} if there is a right context after the punctuation matched by the given
        ///     match or {@code false} when there is no right context.
        /// </summary>
        /// <param name="oneMatch">
        ///     a match matching a punctuation
        /// </param>
        /// <param name="matches">
        ///     a list of all punctuation matching matches
        /// </param>
        /// <param name="i">
        ///     the index of the match in the matches list
        /// </param>
        /// <param name="image">
        ///     the string on which the punctuation matchers have been applied
        /// </param>
        /// <returns> a flag indicating if there is a right context </returns>
        private static bool HasRightContextEnd(Match oneMatch, IList<Match> matches, string image, int i)
        {
            if (i >= matches.Count - 1)
            {
                return oneMatch.EndIndex != image.Length;
            }

            // there is another punctuation later in the image
            var nextMatch = matches[i + 1];
            return nextMatch.StartIndex != oneMatch.EndIndex;
        }


        /// <summary>
        ///     Annotates the given input with the given key value pair at the given range. Also checks if a
        ///     more specific annotation can be found using the token classes matcher.
        /// </summary>
        /// <param name="input">
        ///     the annotated string
        /// </param>
        /// <param name="key">
        ///     the annotation key
        /// </param>
        /// <param name="value">
        ///     the annotation value
        /// </param>
        /// <param name="beginIndex">
        ///     the index of the first character of the range
        /// </param>
        /// <param name="endIndex">
        ///     the index of the character following the last character of the range
        /// </param>
        /// <param name="image">
        ///     the surface image
        /// </param>
        /// <param name="resource">
        ///     the language resource to use
        /// </param>
        private static void Annotate(IAnnotatedString input, string key, object value, int beginIndex, int endIndex,
            string image, LanguageResource resource)
        {
            // get matcher needed for token classes recognition
            var allClassesMatcher = resource.AllClassesMatcher;

            if (allClassesMatcher.matches(image))
            {
                var tokenClass = IdentifyClass(image, allClassesMatcher, resource.ClassesDescription);
                input.Annotate(key, tokenClass, beginIndex, endIndex);
            }
            else
            {
                input.Annotate(key, value, beginIndex, endIndex);
            }
        }


        /// <summary>
        ///     Checks the class of a punctuation and returns the corresponding class name for annotation.
        /// </summary>
        /// <param name="punctuation">
        ///     the match for which to find the class name
        /// </param>
        /// <param name="regExp">
        ///     the regular expression that found the punctuation as a match, {@code null} if
        ///     punctuation wasn't found via a regular expression
        /// </param>
        /// <param name="image">
        ///     a string with the original token containing the punctuation
        /// </param>
        /// <param name="resource">
        ///     a language resource that contains everything needed for identifying the class
        /// </param>
        /// <returns> the class name </returns>
        /// <exception cref="ProcessingException">
        ///     if class of punctuation can't be identified
        /// </exception>
        private static string IdentifyPunctuationClass(Match punctuation, RegExp regExp, string image,
            LanguageResource resource)
        {
            var oneClass = IdentifyClass(punctuation.Image, regExp, resource.PunctuationDescription);
            // check if we have an ambiguous open/close punctuation; if
            // yes, resolve it
            if (resource.IsAncestor(PunctDescription.OPEN_CLOSE_PUNCT, oneClass))
            {
                var nextIndex = punctuation.EndIndex;
                if (nextIndex >= image.Length || !char.IsLetter(image[nextIndex]))
                {
                    oneClass = PunctDescription.CLOSE_PUNCT;
                }
                else
                {
                    var prevIndex = punctuation.StartIndex - 1;
                    if (prevIndex < 0 || !char.IsLetter(image[prevIndex]))
                    {
                        oneClass = PunctDescription.OPEN_PUNCT;
                    }
                }
            }

            // return class name
            return oneClass;
        }


        /// <summary>
        ///     Identifies abbreviations in the annotated token of the given annotated string. Candidates are
        ///     tokens with a followed by a period.
        /// </summary>
        /// <param name="input">An annotated string.</param>
        /// <param name="resource">The language resource to use</param>
        /// <exception cref="ProcessingException">If an error occurs</exception>
        private void IdentifyAbbreviations(IAnnotatedString input, LanguageResource resource)
        {
            // get matchers needed for abbreviation recognition
            var allAbbrevMatcher = resource.AllAbbreviationMatcher;
            var abbreviationLists = resource.AbbreviationLists;

            // iterate over tokens
            var c = input.SetIndex(0);
            // move to first non-whitespace
            if (null == input.GetAnnotation(ClassAnnotation))
            {
                c = input.SetIndex(input.FindNextAnnotation(ClassAnnotation));
            }

            while (c != default)
            {
                // get the end index of the token c belongs to
                var tokenEnd = input.GetRunLimit(ClassAnnotation);

                // get the start index of the token
                var tokenStart = input.Index;
                // set iterator to next non-whitespace token
                c = input.SetIndex(input.FindNextAnnotation(ClassAnnotation));

                // if the next token is a period immediately following the current token,
                // we have found a candidate for an abbreviation
                if (c == '.' && tokenEnd == input.Index)
                {
                    // get the token content WITH the following period
                    tokenEnd = tokenEnd + 1;
                    var image = input.SubString(tokenStart, tokenEnd - tokenStart);

                    // if the abbreviation contains a hyphen, it's sufficient to check
                    // the part after the hyphen
                    var hyphenPos = image.LastIndexOf("-", StringComparison.Ordinal);
                    if (hyphenPos != -1)
                    {
                        var afterHyphen = image.Substring(hyphenPos + 1);
                        if (Regex.IsMatch(afterHyphen, "[^0-9]{2,}"))
                        {
                            image = afterHyphen;
                        }
                    }

                    // check if token is in abbreviation lists
                    var found = false;
                    foreach (var oneEntry in abbreviationLists)
                    {
                        var abbrevClass = oneEntry.Key;
                        var oneList = oneEntry.Value;
                        if (!oneList.Contains(image))
                        {
                            continue;
                        }

                        // annotate abbreviation
                        input.Annotate(ClassAnnotation, abbrevClass, tokenStart, tokenEnd);
                        // stop looking for this abbreviation
                        found = true;
                        break;
                    }

                    if (found)
                    {
                        continue;
                    }

                    // check if token is matched by abbreviation matcher
                    if (!allAbbrevMatcher.matches(image))
                    {
                        continue;
                    }

                    {
                        var abbrevClass = IdentifyClass(image, allAbbrevMatcher, resource.AbbreviationDescription);
                        input.Annotate(ClassAnnotation, abbrevClass, tokenStart, tokenEnd);
                    }
                }
            }
        }


        /// <summary>
        ///     Identifies text units and paragraphs in the given annotated string and annotates them under the
        ///     annotation key BORDER_ANNO.
        /// </summary>
        /// <param name="input">
        ///     an annotated string
        /// </param>
        /// <param name="resource">
        ///     the language resource to use
        /// </param>
        /// <exception cref="ProcessingException">
        ///     if an undefined class name is found
        /// </exception>
        private void IdentifyTus(IAnnotatedString input, LanguageResource resource)
        {
            // get matcher needed for text unit identification
            var internalTuMatcher = resource.InternalTuMatcher;

            // init end-of-sentence-mode flag; when in this mode, every token
            // that is not PTERM, PTERM_P, CLOSE_PUNCT or CLOSE_BRACKET initiates the
            // annotation of a new text unit.
            var eosMode = false;
            var abbrevMode = false;

            // iterate over tokens
            var c = input.SetIndex(0);
            while (c != default)
            {
                var tokenStart = input.GetRunStart(ClassAnnotation);
                var tokenEnd = input.GetRunLimit(ClassAnnotation);
                // check if c belongs to a token
                if (null != input.GetAnnotation(ClassAnnotation))
                {
                    // check if we are in end-of-sentence mode
                    if (eosMode)
                    {
                        // if we find terminal punctuation or closing brackets,
                        // continue with the current sentence
                        if (resource.IsAncestor(PunctDescription.TERM_PUNCT,
                                (string) input.GetAnnotation(ClassAnnotation)) ||
                            resource.IsAncestor(PunctDescription.TERM_PUNCT_P,
                                (string) input.GetAnnotation(ClassAnnotation)) ||
                            resource.IsAncestor(PunctDescription.CLOSE_PUNCT,
                                (string) input.GetAnnotation(ClassAnnotation)) || resource.IsAncestor(
                                PunctDescription.CLOSE_BRACKET, (string) input.GetAnnotation(ClassAnnotation)))
                        {
                            // do nothing
                        }
                        else if (char.IsLower(c) || internalTuMatcher.matches(input.SubString(input.Index, 1)))
                        {
                            // if we find a lower case letter or a punctuation that can
                            // only appear within a text unit, it was wrong alert, the
                            // sentence hasn't ended yet
                            eosMode = false;
                        }
                        else
                        {
                            // otherwise, we just found the first element of the next sentence
                            input.Annotate(BorderAnnotation, TuBorder, tokenStart, tokenStart + 1);
                            eosMode = false;
                        }
                    }
                    else if (abbrevMode)
                    {
                        var image = input.SubString(tokenStart, tokenEnd - tokenStart);
                        if (resource.NonCapitalizedTerms.Contains(image) ||
                            resource.IsAncestor(PunctDescription.OPEN_PUNCT,
                                (string) input.GetAnnotation(ClassAnnotation)))
                        {
                            // there is a term that only starts with a capital letter at the
                            // beginning of a sentence OR
                            // an opening punctuation;
                            // so we just found the first element of the next sentence
                            input.Annotate(BorderAnnotation, TuBorder, tokenStart, tokenStart + 1);
                        }

                        abbrevMode = false;
                        // continue without going to the next token;
                        // it's possible that after an abbreviation follows a
                        // end-of-sentence marker
                        continue;
                    }
                    else
                    {
                        // check if token is a end-of-sentence marker
                        if (resource.IsAncestor(PunctDescription.TERM_PUNCT,
                                (string) input.GetAnnotation(ClassAnnotation)) ||
                            resource.IsAncestor(PunctDescription.TERM_PUNCT_P,
                                (string) input.GetAnnotation(ClassAnnotation)))
                        {
                            eosMode = true;
                        }
                        else if (resource.IsAncestor(AbbrevDescription.B_ABBREVIATION,
                            (string) input.GetAnnotation(ClassAnnotation)))
                        {
                            // check if token is a breaking abbreviation
                            abbrevMode = true;
                        }
                    }

                    // set iterator to next token
                    c = input.SetIndex(tokenEnd);
                }
                else
                {
                    // check for paragraph change in whitespace sequence
                    if (IsParagraphChange(input.SubString(tokenStart, tokenEnd - tokenStart)))
                    {
                        eosMode = false;
                        abbrevMode = false;
                        // set iterator to next token
                        c = input.SetIndex(tokenEnd);
                        // next token starts a new paragraph
                        if (c != default)
                        {
                            input.Annotate(BorderAnnotation, PBorder, input.Index, input.Index + 1);
                        }
                    }
                    else
                    {
                        // just set iterator to next token
                        c = input.SetIndex(tokenEnd);
                    }
                }
            }
        }


        /// <summary>
        ///     Called with a sequence of whitespaces. It returns a flag indicating if the sequence contains a
        ///     paragraph change. A paragraph change is defined as a sequence of whitespaces that contains two
        ///     line breaks.
        /// </summary>
        /// <param name="wSpaces">
        ///     a string consisting only of whitespaces
        /// </param>
        /// <returns> a flag indicating a paragraph change </returns>
        private static bool IsParagraphChange(string wSpaces)
        {
            var len = wSpaces.Length;
            for (var i = 0; i < len; i++)
            {
                var c = wSpaces[i];
                if ('\n' != c && '\r' != c)
                {
                    continue;
                }

                // possible continuations for a paragraph change:
                // - another \n -> paragraph change in Unix or Windows
                // the second \n must no be the next character!
                // this way we catch \n\n for Unix and \r\n\r\n for Windows
                // - another \r -> paragraph change in MacOs or Windows
                // the second \r must no be the next character!
                // this way we catch \r\r for MacOs and \r\n\r\n for Windows
                // we just look for a second occurrence of the c just found
                for (var j = i + 1; j < len; j++)
                {
                    if (c == wSpaces[j])
                    {
                        return true;
                    }
                }
            }

            return false;
        }


        /// <summary>
        ///     Identifies the class of the given string and returns the corresponding class name for
        ///     annotation.
        /// </summary>
        /// <param name="s">
        ///     the string for which to find the class name
        /// </param>
        /// <param name="regExp">
        ///     the regular expression that found the string as a match, {@code null} if string wasn't
        ///     found via a regular expression
        /// </param>
        /// <param name="description">
        ///     a description that contains everything needed for identifying the class
        /// </param>
        /// <returns> the class name </returns>
        /// <exception cref="ProcessingException">
        ///     if class of string can't be identified
        /// </exception>
        private static string IdentifyClass(string s, RegExp regExp, Description description)
        {
            // first try to identify class via the regular expression
            if (null != regExp)
            {
                IDictionary<RegExp, string> regExpMap = description.RegExpMap;
                var oneClass = regExpMap[regExp];
                if (null != oneClass)
                {
                    return oneClass;
                }
            }

            // get hash map with classes
            IDictionary<string, RegExp> definitionsMap = description.DefinitionsMap;
            // iterate over classes
            foreach (var oneEntry in definitionsMap)
            {
                // check if string is of that class
                var oneClass = oneEntry.Key;
                var oneRe = oneEntry.Value;
                if (oneRe.matches(s))
                {
                    // return class name
                    return oneClass;
                }
            }

            // throw exception if no class for string was found
            throw new ProcessingException($"Could not find class for {s}.");
        }


        /// <summary>
        ///     This main method must be used with two or three arguments:
        ///     <ul>
        ///         <li>A file name for the document to tokenize</li>
        ///         <li>The language of the document</li>
        ///         <li>An optional encoding to use (default is UTF-8)</li>
        ///     </ul>
        /// </summary>
        /// <param name="args">
        ///     the arguments
        /// </param>
        public static void Main(string[] args)
        {
            // check for correct arguments
            if (args.Length != 2 && args.Length != 3)
            {
                Console.Write("This method needs two arguments:%n" + "- a file name for the document to tokenize%n" +
                              "- the language of the document%n" + "- an optional encoding to use (default is UTF-8)");
                Environment.Exit(1);
            }

            string text = null;
            try
            {
                // get text from file
                if (File.Exists(args[0]))
                {
                    text = File.ReadAllText(args[0]);
                }
            }
            catch (IOException ioe)
            {
                Console.WriteLine(ioe.ToString());
                Console.Write(ioe.StackTrace);
                Environment.Exit(1);
            }

            try
            {
                // create new instance of NTok
                var testTok = new NTok();

                // tokenize text
                var result = testTok.Tokenize(text, args[1]);

                // print result
                foreach (var onePara in Outputter.createParagraphs(result))
                {
                    Console.WriteLine(onePara);
                }
            }
            catch (IOException e)
            {
                Console.Error.WriteLine(e.Message);
            }
        }
    }
}