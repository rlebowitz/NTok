﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using NetTok.Tokenizer.Annotate;
using NetTok.Tokenizer.Descriptions;
using NetTok.Tokenizer.Exceptions;
using NetTok.Tokenizer.Output;
using NetTok.Tokenizer.Utilities;
[assembly: InternalsVisibleTo("NetTok.Tests.Integration")]

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
            var loggerFactory = (ILoggerFactory)new LoggerFactory();
            _logger = new Logger<NTok>(loggerFactory);

            LanguageResources = new Dictionary<string, LanguageResource>();
            using var reader = new StreamReader(ResourceManager.Read("ntok.cfg"));
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
            IdentifyTextUnits(input, resource);
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
        internal void IdentifyTokens(IAnnotatedString input, LanguageResource resource)
        {
            // init token start index
            var tokenStart = 0;
            // flag for indicating if new token was found
            var tokenFound = false;
            // get classes root annotation
            var rootClass = resource.ClassesRoot.TagName();
            int index;
            for (index = 0; index < input.Length; index++)
            {
                if (char.IsWhiteSpace(input[index]) || input[index] == '\u00a0')
                {
                    if (!tokenFound)
                    {
                        continue;
                    }

                    var token = input.Substring(tokenStart, index);
                    Annotate(input, ClassAnnotation, rootClass, tokenStart, index, token, resource);
                    tokenFound = false;
                }
                else if (!tokenFound)
                {
                    // a new token starts here, after some whitespaces
                    tokenFound = true;
                    tokenStart = index;
                }
            }

            // annotate last token
            if (tokenFound)
            {
                Annotate(input, ClassAnnotation, rootClass, tokenStart, index, input.Substring(tokenStart, index), resource);
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
        internal void IdentifyPunctuation(IAnnotatedString input, LanguageResource resource)
        {
            // get the matchers needed
            var punctuationMatcher = resource.AllPunctuationMatcher;
            var internalMatcher = resource.InternalMatcher;

            // get the class of the root element of the class hierarchy;
            // only tokens with this type are further examined
            var rootClass = resource.ClassesRoot.TagName();

            // iterate over tokens
            //input.Index = 0;
            var idx = 0;
            //var c = input[input.Index];
            // move to first non-whitespace
            if (input.GetAnnotation(ClassAnnotation) == null)
            {
                //c = input.SetIndex(input.FindNextAnnotation(ClassAnnotation));
                idx = input.FindNextAnnotation(ClassAnnotation);
            }

            while (idx < input.Length)
            {
                // only check tokens
                if (null == input.GetAnnotation(ClassAnnotation))
                {
 //                   c = input.SetIndex(input.FindNextAnnotation(ClassAnnotation));
                    idx = input.FindNextAnnotation(ClassAnnotation);
                    continue;
                }

                // get class of token
                var tokClass = (string)input.GetAnnotation(ClassAnnotation);
                // only check tokens with the most general class
                if (tokClass != rootClass)
                {
//                    c = input.SetIndex(input.FindNextAnnotation(ClassAnnotation));
                    idx = input.FindNextAnnotation(ClassAnnotation);
                    continue;
                }

                // save the next token start position;
                // required because the input index might be changed later in this method
                var nextTokenStart = input.FindNextAnnotation(ClassAnnotation);

                // split punctuation on the left and right side of the token
                SplitPunctuation(input, resource);

                // update current token annotation
                tokClass = (string)input.GetAnnotation(ClassAnnotation);
                // only check tokens with the most general class
                if (tokClass != rootClass)
                {
//                    c = input.SetIndex(nextTokenStart);
                    idx = nextTokenStart;
                    continue;
                }

                // split clitics from left and right side of the token
                SplitClitics(input, resource);

                // update current token annotation
                tokClass = (string)input.GetAnnotation(ClassAnnotation);
                // only check tokens with the most general class
                if (tokClass != rootClass)
                {
//                    c = input.SetIndex(nextTokenStart);
                    idx = nextTokenStart;
                    continue;
                }

                // get the start index of the token
                var tokenStart = idx; //input.Index;
                // get the end index of the token c belongs to
                var tokenEnd = input.GetRunLimit(ClassAnnotation);
                // get the token content
                var content = input.Substring(tokenStart, tokenEnd);

                // use the all rule to split image in parts consisting of
                // punctuation and non-punctuation
                var matches = punctuationMatcher.GetAllMatches(content);
                // if there is no punctuation just continue
                if (0 == matches.Count)
                {
//                    c = input.SetIndex(nextTokenStart);
                    idx = nextTokenStart;
                    continue;
                }

                // this is the relative start position of current token within
                // the image
                var index = 0;
                // iterator over matches
                for (var i = 0; i < matches.Count; i++)
                {
                    // get next match
                    var match = matches[i];

                    // check if we have some non-punctuation before the current
                    // punctuation
                    if (index != match.Index)
                    {
                        // check for internal punctuation:
                        if (internalMatcher.IsMatch(match.Value))
                        {
                            // punctuation is internal;
                            // check for right context
                            if (HasRightContextEnd(match, matches, content, i))
                            {
                                // token not complete yet
                                continue;
                            }
                        }

                        // we have a breaking punctuation; create token for
                        // non-punctuation before the current punctuation
                        Annotate(input, ClassAnnotation, tokClass, tokenStart + index, tokenStart + match.Index,
                            content[index..match.Index], resource);
                        index = match.Index;
                    }

                    // punctuation is not internal:
                    // get the class of the punctuation and create token for it
                    var punctuationClass = IdentifyPunctuationClass(match, null, content, resource);
                    input.Annotate(ClassAnnotation, punctuationClass, tokenStart + index, tokenStart + match.Index);
                    index = match.EndIndex();
                }

                // cleanup after all matches have been processed
                if (index != content.Length)
                {
                    // create a token from rest of image
                    Annotate(input, ClassAnnotation, tokClass, tokenStart + index, tokenStart + content.Length,
                        content[index..], resource);
                }

                // set iterator to next non-whitespace token
              //  c = input.SetIndex(nextTokenStart);
              idx = nextTokenStart;
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
            var content = input.Substring(tokenStart, tokenEnd);
            // get current token annotation
            var tokClass = (string)input.GetAnnotation(ClassAnnotation);

            // check for punctuation at the beginning of the token
            var startMatch = punctuationMatcher.Starts(content);
            while (null != startMatch)
            {
                // create token for punctuation
                var punctuationClass = IdentifyPunctuationClass(startMatch, null, content, resource);
                input.Annotate(ClassAnnotation, punctuationClass, tokenStart + startMatch.Index,
                    tokenStart + startMatch.EndIndex());
                tokenStart += startMatch.EndIndex();
                content = input.Substring(tokenStart, tokenEnd);
                input.Index = tokenStart;
                if (content.Length > 0)
                {
                    Annotate(input, ClassAnnotation, tokClass, tokenStart, tokenEnd, content, resource);
                    tokClass = (string)input.GetAnnotation(ClassAnnotation);
                    if (tokClass != rootClass)
                    {
                        // the remaining token could be matched with a non-root class,
                        // so stop splitting punctuation
                        break;
                    }

                    startMatch = punctuationMatcher.Starts(content);
                }
                else
                {
                    startMatch = null;
                }
            }

            // check for punctuation at the end of the token
            var endMatch = punctuationMatcher.Ends(content);
            while (null != endMatch)
            {
                // create token for punctuation
                var punctuationClass = IdentifyPunctuationClass(endMatch, null, content, resource);
                input.Annotate(ClassAnnotation, punctuationClass, tokenStart + endMatch.Index,
                    tokenStart + endMatch.EndIndex());
                tokenEnd = tokenStart + endMatch.Index;
                content = input.Substring(tokenStart, tokenEnd);
                if (content.Length > 0)
                {
                    Annotate(input, ClassAnnotation, tokClass, tokenStart, tokenEnd, content, resource);
                    tokClass = (string)input.GetAnnotation(ClassAnnotation);
                    if (tokClass != rootClass)
                    {
                        // the remaining token could be matched with a non-root class, so stop splitting punctuation
                        break;
                    }

                    endMatch = punctuationMatcher.Ends(content);
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
            var image = input.Substring(tokenStart, tokenEnd);
            // get current token annotation
            var tokClass = (string)input.GetAnnotation(ClassAnnotation);

            // check for proclitics
            var proclitic = procliticsMatcher.Starts(image);
            // create token for proclitic
            while (null != proclitic)
            {
                var identifyClass = IdentifyClass(proclitic.Value, procliticsMatcher, resource.CliticsDescription);
                input.Annotate(ClassAnnotation, identifyClass, tokenStart + proclitic.Index,
                    tokenStart + proclitic.EndIndex());
                tokenStart += proclitic.EndIndex();
                image = input.Substring(tokenStart, tokenEnd);
                input.Index = tokenStart;
                if (image.Length > 0)
                {
                    Annotate(input, ClassAnnotation, tokClass, tokenStart, tokenEnd, image, resource);
                    tokClass = (string)input.GetAnnotation(ClassAnnotation);
                    if (tokClass != rootClass)
                    {
                        // the remaining token could be matched with a non-root class,
                        // so stop splitting proclitics
                        break;
                    }

                    proclitic = procliticsMatcher.Starts(image);
                }
                else
                {
                    proclitic = null;
                }
            }

            // check for enclitics
            var enclitic = encliticsMatcher.Ends(image);
            while (null != enclitic)
            {
                // create tokens for enclitic
                var cliticClass = IdentifyClass(enclitic.Value, encliticsMatcher, resource.CliticsDescription);
                input.Annotate(ClassAnnotation, cliticClass, tokenStart + enclitic.Index,
                    tokenStart + enclitic.EndIndex());
                tokenEnd = tokenStart + enclitic.Index;
                image = input.Substring(tokenStart, tokenEnd);
                if (image.Length > 0)
                {
                    Annotate(input, ClassAnnotation, tokClass, tokenStart, tokenEnd, image, resource);
                    tokClass = (string)input.GetAnnotation(ClassAnnotation);
                    if (tokClass != rootClass)
                    {
                        // the remaining token could be matched with a non-root class,
                        // so stop splitting enclitics
                        break;
                    }

                    enclitic = encliticsMatcher.Ends(image);
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
                return oneMatch.EndIndex() != image.Length;
            }

            // there is another punctuation later in the image
            var nextMatch = matches[i + 1];
            return nextMatch.Index != oneMatch.EndIndex();
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
        /// <param name="content">
        ///     the surface image (token?)
        /// </param>
        /// <param name="resource">The specified language resource.</param>
        //private static void Annotate(IAnnotatedString input, string key, object value, int beginIndex, int endIndex,
        //    string content, LanguageResource resource)
        internal static void Annotate(IAnnotatedString input, string key, string value, int beginIndex, int endIndex,
            string content, LanguageResource resource)
        {
            // get matcher needed for token classes recognition
            var allClassesMatcher = resource.AllClassesMatcher;
            if (allClassesMatcher.IsMatch(content))
            {
                var tokenClass = IdentifyClass(content, allClassesMatcher, resource.ClassesDescription);
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
        /// <param name="regex">
        ///     the regular expression that found the punctuation as a match, {@code null} if
        ///     punctuation wasn't found via a regular expression
        /// </param>
        /// <param name="content">
        ///     a string with the original token containing the punctuation
        /// </param>
        /// <param name="resource">
        ///     a language resource that contains everything needed for identifying the class
        /// </param>
        /// <returns> the class name </returns>
        /// <exception cref="ProcessingException">
        ///     if class of punctuation can't be identified
        /// </exception>
        internal static string IdentifyPunctuationClass(Match punctuation, Regex regex, string content,
            LanguageResource resource)
        {
            var oneClass = IdentifyClass(punctuation.Value, regex, resource.PunctuationDescription);
            // check if we have an ambiguous open/close punctuation; if
            // yes, resolve it
            if (resource.IsAncestor(Constants.Punctuation.OpenClosePunct, oneClass))
            {
                var nextIndex = punctuation.EndIndex();
                if (nextIndex >= content.Length || !char.IsLetter(content[nextIndex]))
                {
                    oneClass = Constants.Punctuation.ClosePunct;
                }
                else
                {
                    var prevIndex = punctuation.Index - 1;
                    if (prevIndex < 0 || !char.IsLetter(content[prevIndex]))
                    {
                        oneClass = Constants.Punctuation.OpenPunct;
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
        internal void IdentifyAbbreviations(IAnnotatedString input, LanguageResource resource)
        {
            // get matchers needed for abbreviation recognition
            var allAbbrevMatcher = resource.AllAbbreviationMatcher;
            var abbreviationMap = resource.AbbreviationMap;

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
                    tokenEnd += 1;
                    var content = input.Substring(tokenStart, tokenEnd);

                    // if the abbreviation contains a hyphen, it's sufficient to check
                    // the part after the hyphen
                    var hyphenPos = content.LastIndexOf("-", StringComparison.Ordinal);
                    if (hyphenPos != -1)
                    {
                        var afterHyphen = content[(hyphenPos + 1)..];
                        if (Regex.IsMatch(afterHyphen, "[^0-9]{2,}"))
                        {
                            content = afterHyphen;
                        }
                    }

                    // check if token is in abbreviation lists
                    var found = false;
                    foreach (var (key, value) in abbreviationMap)
                    {
                        //                        var abbrevClass = abbreviation.Key;
                        //                        var oneList = abbreviation.Value;
                        if (!value.Contains(content))
                        {
                            continue;
                        }

                        // annotate abbreviation
                        input.Annotate(ClassAnnotation, key, tokenStart, tokenEnd);
                        // stop looking for this abbreviation
                        found = true;
                        break;
                    }

                    if (found)
                    {
                        continue;
                    }

                    // check if token is matched by abbreviation matcher
                    if (!allAbbrevMatcher.IsMatch(content))
                    {
                        continue;
                    }

                    {
                        var abbrevClass = IdentifyClass(content, allAbbrevMatcher, resource.AbbreviationDescription);
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
        internal void IdentifyTextUnits(IAnnotatedString input, LanguageResource resource)
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
                        if (resource.IsAncestor(Constants.Punctuation.TermPunct,
                                (string)input.GetAnnotation(ClassAnnotation)) ||
                            resource.IsAncestor(Constants.Punctuation.TermPunctP,
                                (string)input.GetAnnotation(ClassAnnotation)) ||
                            resource.IsAncestor(Constants.Punctuation.ClosePunct,
                                (string)input.GetAnnotation(ClassAnnotation)) || resource.IsAncestor(
                                Constants.Punctuation.CloseBracket, (string)input.GetAnnotation(ClassAnnotation)))
                        {
                            // do nothing
                        }
                        else if (char.IsLower(c) || internalTuMatcher.IsMatch(input.Substring(input.Index, 1)))
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
                        var image = input.Substring(tokenStart, tokenEnd);
                        if (resource.NonCapitalizedTerms.Contains(image) ||
                            resource.IsAncestor(Constants.Punctuation.OpenPunct,
                                (string)input.GetAnnotation(ClassAnnotation)))
                        {
                            // there is a term that only starts with a capital letter at the
                            // beginning of a sentence OR an opening punctuation;
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
                        if (resource.IsAncestor(Constants.Punctuation.TermPunct,
                                (string)input.GetAnnotation(ClassAnnotation)) ||
                            resource.IsAncestor(Constants.Punctuation.TermPunctP,
                                (string)input.GetAnnotation(ClassAnnotation)))
                        {
                            eosMode = true;
                        }
                        else if (resource.IsAncestor(Constants.Abbreviations.BAbbreviation,
                            (string)input.GetAnnotation(ClassAnnotation)))
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
                    if (IsParagraphChange(input.Substring(tokenStart, tokenEnd)))
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
        /// <param name="regex">
        ///     the regular expression that found the string as a match, null if string wasn't
        ///     found via a regular expression
        /// </param>
        /// <param name="description">
        ///     A description that contains everything needed for identifying the class.
        /// </param>
        /// <returns>The string's class name.</returns>
        /// <exception cref="ProcessingException">Thrown if the string's class can't be identified.</exception>
        internal static string IdentifyClass(string s, Regex regex, Description description)
        {
            // Using the Regex -> TokenClassName map, try to identify the string's class using
            // the regular expression that matched the input string, assuming the regular expression exists.
            if (regex != null)
            {
                if (description.RegexTokenClassMap.ContainsKey(regex))
                {
                    return description.RegexTokenClassMap[regex];
                }
            }

            // get hash map with classes
            var definitionsMap = description.DefinitionsMap;
            // iterate over classes
            foreach (var (@class, regexValue) in definitionsMap)
            {
                // check if string is of that class
                if (regexValue.IsMatch(s))
                {
                    return @class;
                }
            }

            // throw exception if no class for string was found
            throw new ProcessingException($"Could not find a matching class for {s}.");
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
                Console.WriteLine("This method needs two arguments:");
                Console.WriteLine("- a file name for the document to tokenize");
                Console.WriteLine("- the language of the document");
                Console.WriteLine("- an optional encoding to use (default is UTF-8)");
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
                foreach (var onePara in Outputter.CreateParagraphs(result))
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