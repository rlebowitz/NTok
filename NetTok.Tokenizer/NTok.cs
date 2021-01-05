using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using NetTok.Tokenizer.annotate;
using NetTok.Tokenizer.exceptions;
using NetTok.Tokenizer.output;
using NetTok.Tokenizer.regexp;

/*
 * JTok
 * A configurable tokenizer implemented in Java
 *
 * (C) 2003 - 2014  DFKI Language Technology Lab http://www.dfki.de/lt
 *   Author: Joerg Steffen, steffen@dfki.de
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
    /// Tokenizer tool that recognizes paragraphs, sentences, tokens, punctuation, numbers,
    /// abbreviations, etc.
    /// 
    /// @author Joerg Steffen, DFKI
    /// </summary>
    /// <remarks>
    /// https://stackoverflow.com/questions/46483019/logging-from-static-members-with-microsoft-extensions-logging
    /// </remarks>
    public class NTok
    {
        private readonly ILogger _logger;
        /// <summary>
        /// annotation key for the token class </summary>
        public const string CLASS_ANNO = "class";

        /// <summary>
        /// annotation key for sentences and paragraph borders </summary>
        public const string BORDER_ANNO = "border";

        /// <summary>
        /// annotation value for text unit borders </summary>
        public const string TU_BORDER = "tu";

        /// <summary>
        /// annotation value for paragraph borders </summary>
        public const string P_BORDER = "p";


        // the logger
        //  private static readonly Logger logger = LoggerFactory.getLogger(typeof(JTok));

        // identifier of the default configuration
        private const string DEFAULT = "default";


        // maps each supported language to a language resource
        private IDictionary<string, LanguageResource> langResources;


        /// <summary>
        /// Creates a new instance of <seealso cref="NTok"/>.
        /// </summary>
        /// <exception cref="IOException">
        ///           if there is an error reading the configuration
        /// </exception>
        public NTok()
        {
            var loggerFactory = (ILoggerFactory)new LoggerFactory();
            _logger = new Logger<NTok>(loggerFactory);

            langResources = new Dictionary<string, LanguageResource>();



            foreach (KeyValuePair<object, object> oneEntry in configProps.entrySet())
            {
                // get language
                string oneLanguage = (string)oneEntry.Key;
                // add language resources for that language
                string langDir = (string)oneEntry.Value;
                _logger.LogInformation(string.Format("loading language resources for {0} from {1}", oneLanguage, langDir));
                this.langResources[oneLanguage] = new LanguageResource(oneLanguage, langDir);
            }
        }


        /// <summary>
        /// Returns the language resource for the given language if available.
        /// </summary>
        /// <param name="lang">
        ///          the language </param>
        /// <returns> the language resource or the default configuration if language is not supported </returns>
        public virtual LanguageResource getLanguageResource(string lang)
        {

            object probe = this.langResources[lang];
            if (null != probe)
            {
                return (LanguageResource)probe;
            }
            _logger.LogInformation(string.Format("language {0} not supported, using default configuration", lang));
            return this.langResources[DEFAULT];
        }


        /// <summary>
        /// Tokenizes the given text in the given language. Returns an annotated string containing the
        /// identified paragraphs with their text units and tokens.<br>
        /// This method is thread-safe.
        /// </summary>
        /// <param name="inputText">
        ///          the text to tokenize </param>
        /// <param name="lang">
        ///          the language of the text </param>
        /// <returns> an annotated string </returns>
        /// <exception cref="ProcessingException">
        ///              if input data causes an error e.g. if language is not supported </exception>
        public virtual AnnotatedString Tokenize(string inputText, string lang)
        {

            // get language resource for language
            LanguageResource langRes = this.getLanguageResource(lang);

            // init attributed string for annotation
            AnnotatedString input = new FastAnnotatedString(inputText);

            // identify tokens
            this.IdentifyTokens(input, langRes);

            // identify punctuation
            this.IdentifyPunct(input, langRes);

            // identify abbreviations
            this.IdentifyAbbrev(input, langRes);

            // identify sentences and paragraphs
            this.IdentifyTus(input, langRes);

            // return result
            return input;
        }


        /// <summary>
        /// Identifies tokens and annotates them. Tokens are sequences of non-whitespaces.
        /// </summary>
        /// <param name="input">
        ///          an annotated string </param>
        /// <param name="langRes">
        ///          the language resource to use </param>
        private void IdentifyTokens(AnnotatedString input, LanguageResource langRes)
        {

            // init token start index
            int tokenStart = 0;
            // flag for indicating if new token was found
            bool tokenFound = false;

            // get classes root annotation
            string rootClass = langRes.ClassesRoot.TagName;

            // iterate over input
            for (char c = input.first(); c != CharacterIterator.DONE; c = input.next())
            {
                if (char.IsWhiteSpace(c) || (c == '\u00a0'))
                {
                    if (tokenFound)
                    {
                        // annotate newly identified token
                        this.Annotate(input, CLASS_ANNO, rootClass, tokenStart, input.Index, input.substring(tokenStart, input.Index - tokenStart), langRes);
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
                this.Annotate(input, CLASS_ANNO, rootClass, tokenStart, input.Index, input.substring(tokenStart, input.Index - tokenStart), langRes);
            }
        }


        /// <summary>
        /// Identifies punctuations in the annotated tokens of the given annotated string
        /// </summary>
        /// <param name="input">
        ///          an annotated string </param>
        /// <param name="langRes">
        ///          the language resource to use </param>
        /// <exception cref="ProcessingException">
        ///              if an error occurs </exception>
        private void IdentifyPunct(AnnotatedString input, LanguageResource langRes)
        {

            // get the matchers needed
            RegExp allPunctMatcher = langRes.AllPunctuationMatcher;
            RegExp internalMatcher = langRes.InternalMatcher;

            // get the class of the root element of the class hierarchy;
            // only tokens with this type are further examined
            string rootClass = langRes.ClassesRoot.TagName;

            // iterate over tokens
            char c = input.setIndex(0);
            // move to first non-whitespace
            if (null == input.getAnnotation(CLASS_ANNO))
            {
                c = input.setIndex(input.findNextAnnotation(CLASS_ANNO));
            }
            while (c != CharacterIterator.DONE)
            {

                // only check tokens
                if (null == input.getAnnotation(CLASS_ANNO))
                {
                    c = input.setIndex(input.findNextAnnotation(CLASS_ANNO));
                    continue;
                }

                // get class of token
                string tokClass = (string)input.getAnnotation(CLASS_ANNO);
                // only check tokens with the most general class
                if (!string.ReferenceEquals(tokClass, rootClass))
                {
                    c = input.setIndex(input.findNextAnnotation(CLASS_ANNO));
                    continue;
                }

                // save the next token start position;
                // required because the input index might be changed later in this method
                int nextTokenStart = input.findNextAnnotation(CLASS_ANNO);

                // split punctuation on the left and right side of the token
                this.SplitPunctuation(input, langRes);

                // update current token annotation
                tokClass = (string)input.getAnnotation(CLASS_ANNO);
                // only check tokens with the most general class
                if (!string.ReferenceEquals(tokClass, rootClass))
                {
                    c = input.setIndex(nextTokenStart);
                    continue;
                }

                // split clitics from left and right side of the token
                this.splitClitics(input, langRes);

                // update current token annotation
                tokClass = (string)input.getAnnotation(CLASS_ANNO);
                // only check tokens with the most general class
                if (!string.ReferenceEquals(tokClass, rootClass))
                {
                    c = input.setIndex(nextTokenStart);
                    continue;
                }

                // get the start index of the token
                int tokenStart = input.Index;
                // get the end index of the token c belongs to
                int tokenEnd = input.getRunLimit(CLASS_ANNO);
                // get the token content
                string image = input.substring(tokenStart, tokenEnd - tokenStart);

                // use the all rule to split image in parts consisting of
                // punctuation and non-punctuation
                IList<Match> matches = allPunctMatcher.getAllMatches(image);
                // if there is no punctuation just continue
                if (0 == matches.Count)
                {
                    c = input.setIndex(nextTokenStart);
                    continue;
                }

                // this is the relative start position of current token within
                // the image
                int index = 0;
                // iterator over matches
                for (int i = 0; i < matches.Count; i++)
                {
                    // get next match
                    Match oneMatch = matches[i];

                    // check if we have some non-punctuation before the current
                    // punctuation
                    if (index != oneMatch.StartIndex)
                    {
                        // check for internal punctuation:
                        if (internalMatcher.matches(oneMatch.Image))
                        {
                            // punctuation is internal;
                            // check for right context
                            if (this.HasRightContextEnd(oneMatch, matches, image, i))
                            {
                                // token not complete yet
                                continue;
                            }
                        }

                        // we have a breaking punctuation; create token for
                        // non-punctuation before the current punctuation
                        this.Annotate(input, CLASS_ANNO, tokClass, tokenStart + index, tokenStart + oneMatch.StartIndex, image.Substring(index, oneMatch.StartIndex - index), langRes);
                        index = oneMatch.StartIndex;
                    }

                    // punctuation is not internal:
                    // get the class of the punctuation and create token for it
                    string punctClass = this.IdentifyPunctClass(oneMatch, null, image, langRes);
                    input.annotate(CLASS_ANNO, punctClass, tokenStart + index, tokenStart + oneMatch.EndIndex);
                    index = oneMatch.EndIndex;
                }

                // cleanup after all matches have been processed
                if (index != image.Length)
                {
                    // create a token from rest of image
                    this.Annotate(input, CLASS_ANNO, tokClass, tokenStart + index, tokenStart + image.Length, image.Substring(index), langRes);
                }

                // set iterator to next non-whitespace token
                c = input.setIndex(nextTokenStart);
            }
        }


        /// <summary>
        /// Splits punctuation from the left and right side of the token if possible.
        /// </summary>
        /// <param name="input">
        ///          the annotate string </param>
        /// <param name="langRes">
        ///          the language resource to use </param>
        private void SplitPunctuation(AnnotatedString input, LanguageResource langRes)
        {

            // get the matchers needed
            RegExp allPunctMatcher = langRes.AllPunctuationMatcher;

            // get the class of the root element of the class hierarchy;
            // only tokens with this type are further examined
            string rootClass = langRes.ClassesRoot.TagName;

            // get the start index of the token
            int tokenStart = input.Index;
            // get the end index of the token
            int tokenEnd = input.getRunLimit(CLASS_ANNO);
            // get the token content
            string image = input.substring(tokenStart, tokenEnd - tokenStart);
            // get current token annotation
            string tokClass = (string)input.getAnnotation(CLASS_ANNO);

            // check for punctuation at the beginning of the token
            Match startMatch = allPunctMatcher.starts(image);
            while (null != startMatch)
            {
                // create token for punctuation
                string punctClass = this.IdentifyPunctClass(startMatch, null, image, langRes);
                input.annotate(CLASS_ANNO, punctClass, tokenStart + startMatch.StartIndex, tokenStart + startMatch.EndIndex);
                tokenStart = tokenStart + startMatch.EndIndex;
                image = input.substring(tokenStart, tokenEnd - tokenStart);
                input.Index = tokenStart;
                if (image.Length > 0)
                {
                    this.Annotate(input, CLASS_ANNO, tokClass, tokenStart, tokenEnd, image, langRes);
                    tokClass = (string)input.getAnnotation(CLASS_ANNO);
                    if (!string.ReferenceEquals(tokClass, rootClass))
                    {
                        // the remaining token could be matched with a non-root class,
                        // so stop splitting punctuation
                        break;
                    }
                    startMatch = allPunctMatcher.starts(image);
                }
                else
                {
                    startMatch = null;
                }
            }

            // check for punctuation at the end of the token
            Match endMatch = allPunctMatcher.ends(image);
            while (null != endMatch)
            {
                // create token for punctuation
                string punctClass = this.IdentifyPunctClass(endMatch, null, image, langRes);
                input.annotate(CLASS_ANNO, punctClass, tokenStart + endMatch.StartIndex, tokenStart + endMatch.EndIndex);
                tokenEnd = tokenStart + endMatch.StartIndex;
                image = input.substring(tokenStart, tokenEnd - tokenStart);
                if (image.Length > 0)
                {
                    this.Annotate(input, CLASS_ANNO, tokClass, tokenStart, tokenEnd, image, langRes);
                    tokClass = (string)input.getAnnotation(CLASS_ANNO);
                    if (!string.ReferenceEquals(tokClass, rootClass))
                    {
                        // the remaining token could be matched with a non-root class,
                        // so stop splitting punctuation
                        break;
                    }
                    endMatch = allPunctMatcher.ends(image);
                }
                else
                {
                    endMatch = null;
                }
            }
        }


        /// <summary>
        /// Splits pro- and enclitics from the left and right side of the token if possible.
        /// </summary>
        /// <param name="input">
        ///          the annotate string </param>
        /// <param name="langRes">
        ///          the language resource to use </param>
        private void splitClitics(AnnotatedString input, LanguageResource langRes)
        {

            // get matchers needed for clitics recognition
            RegExp proclitMatcher = langRes.ProcliticsMatcher;
            RegExp enclitMatcher = langRes.EncliticsMatcher;

            // get the class of the root element of the class hierarchy;
            // only tokens with this type are further examined
            string rootClass = langRes.ClassesRoot.TagName;

            // get the start index of the token
            int tokenStart = input.Index;
            // get the end index of the token c belongs to
            int tokenEnd = input.getRunLimit(CLASS_ANNO);
            // get the token content
            string image = input.substring(tokenStart, tokenEnd - tokenStart);
            // get current token annotation
            string tokClass = (string)input.getAnnotation(CLASS_ANNO);

            // check for proclitics
            Match proclit = proclitMatcher.starts(image);
            // create token for proclitic
            while (null != proclit)
            {
                string clitClass = IdentifyClass(proclit.Image, proclitMatcher, langRes.CliticsDescription);
                input.annotate(CLASS_ANNO, clitClass, tokenStart + proclit.StartIndex, tokenStart + proclit.EndIndex);
                tokenStart = tokenStart + proclit.EndIndex;
                image = input.substring(tokenStart, tokenEnd - tokenStart);
                input.Index = tokenStart;
                if (image.Length > 0)
                {
                    this.Annotate(input, CLASS_ANNO, tokClass, tokenStart, tokenEnd, image, langRes);
                    tokClass = (string)input.getAnnotation(CLASS_ANNO);
                    if (!string.ReferenceEquals(tokClass, rootClass))
                    {
                        // the remaining token could be matched with a non-root class,
                        // so stop splitting proclitics
                        break;
                    }
                    proclit = proclitMatcher.starts(image);
                }
                else
                {
                    proclit = null;
                }
            }

            // check for enclitics
            Match enclit = enclitMatcher.ends(image);
            while (null != enclit)
            {
                // create tokens for enclitic
                string clitClass = IdentifyClass(enclit.Image, enclitMatcher, langRes.CliticsDescription);
                input.annotate(CLASS_ANNO, clitClass, tokenStart + enclit.StartIndex, tokenStart + enclit.EndIndex);
                tokenEnd = tokenStart + enclit.StartIndex;
                image = input.substring(tokenStart, tokenEnd - tokenStart);
                if (image.Length > 0)
                {
                    this.Annotate(input, CLASS_ANNO, tokClass, tokenStart, tokenEnd, image, langRes);
                    tokClass = (string)input.getAnnotation(CLASS_ANNO);
                    if (!string.ReferenceEquals(tokClass, rootClass))
                    {
                        // the remaining token could be matched with a non-root class,
                        // so stop splitting enclitics
                        break;
                    }
                    enclit = enclitMatcher.ends(image);
                }
                else
                {
                    enclit = null;
                }
            }
        }


        /// <summary>
        /// Returns {@code true} if there is a right context after the punctuation matched by the given
        /// match or {@code false} when there is no right context.
        /// </summary>
        /// <param name="oneMatch">
        ///          a match matching a punctuation </param>
        /// <param name="matches">
        ///          a list of all punctuation matching matches </param>
        /// <param name="i">
        ///          the index of the match in the matches list </param>
        /// <param name="image">
        ///          the string on which the punctuation matchers have been applied </param>
        /// <returns> a flag indicating if there is a right context </returns>
        private bool HasRightContextEnd(Match oneMatch, IList<Match> matches, string image, int i)
        {

            if (i < (matches.Count - 1))
            {
                // there is another punctuation later in the image
                Match nextMatch = matches[i + 1];
                if (nextMatch.StartIndex != oneMatch.EndIndex)
                {
                    // there is some right context and punctuation
                    // following the internal punctuation
                    return true;
                }
                return false;
            }
            return oneMatch.EndIndex != image.Length;
        }


        /// <summary>
        /// Annotates the given input with the given key value pair at the given range. Also checks if a
        /// more specific annotation can be found using the token classes matcher.
        /// </summary>
        /// <param name="input">
        ///          the annotated string </param>
        /// <param name="key">
        ///          the annotation key </param>
        /// <param name="value">
        ///          the annotation value </param>
        /// <param name="beginIndex">
        ///          the index of the first character of the range </param>
        /// <param name="endIndex">
        ///          the index of the character following the last character of the range </param>
        /// <param name="image">
        ///          the surface image </param>
        /// <param name="langRes">
        ///          the language resource to use </param>
        private void Annotate(AnnotatedString input, string key, object value, int beginIndex, int endIndex, string image, LanguageResource langRes)
        {

            // get matcher needed for token classes recognition
            RegExp allClassesMatcher = langRes.AllClassesMatcher;

            if (allClassesMatcher.matches(image))
            {
                string tokenClass = IdentifyClass(image, allClassesMatcher, langRes.ClassesDescription);
                input.annotate(key, tokenClass, beginIndex, endIndex);
            }
            else
            {
                input.annotate(key, value, beginIndex, endIndex);
            }
        }


        /// <summary>
        /// Checks the class of a punctuation and returns the corresponding class name for annotation.
        /// </summary>
        /// <param name="punct">
        ///          the match for which to find the class name </param>
        /// <param name="regExp">
        ///          the regular expression that found the punctuation as a match, {@code null} if
        ///          punctuation wasn't found via a regular expression </param>
        /// <param name="image">
        ///          a string with the original token containing the punctuation </param>
        /// <param name="langRes">
        ///          a language resource that contains everything needed for identifying the class </param>
        /// <returns> the class name </returns>
        /// <exception cref="ProcessingException">
        ///              if class of punctuation can't be identified </exception>
        private string IdentifyPunctClass(Match punct, RegExp regExp, string image, LanguageResource langRes)
        {

            string oneClass = IdentifyClass(punct.Image, regExp, langRes.PunctuationDescription);
            // check if we have an ambiguous open/close punctuation; if
            // yes, resolve it
            if (langRes.IsAncestor(PunctDescription.OPEN_CLOSE_PUNCT, oneClass))
            {

                int nextIndex = punct.EndIndex;
                if ((nextIndex >= image.Length) || !char.IsLetter(image[nextIndex]))
                {
                    oneClass = PunctDescription.CLOSE_PUNCT;
                }
                else
                {
                    int prevIndex = punct.StartIndex - 1;
                    if ((prevIndex < 0) || !char.IsLetter(image[prevIndex]))
                    {
                        oneClass = PunctDescription.OPEN_PUNCT;
                    }
                }
            }
            // return class name
            return oneClass;
        }


        /// <summary>
        /// Identifies abbreviations in the annotated token of the given annotated string. Candidates are
        /// tokens with a followed by a period.
        /// </summary>
        /// <param name="input">
        ///          an annotated string </param>
        /// <param name="langRes">
        ///          the language resource to use </param>
        /// <exception cref="ProcessingException">
        ///              if an error occurs </exception>
        private void IdentifyAbbrev(AnnotatedString input, LanguageResource langRes)
        {

            // get matchers needed for abbreviation recognition
            RegExp allAbbrevMatcher = langRes.AllAbbreviationMatcher;

            // get map with abbreviation lists
            IDictionary<string, ISet<string>> abbrevLists = langRes.AbbreviationLists;

            // iterate over tokens
            char c = input.setIndex(0);
            // move to first non-whitespace
            if (null == input.getAnnotation(CLASS_ANNO))
            {
                c = input.setIndex(input.findNextAnnotation(CLASS_ANNO));
            }
            while (c != CharacterIterator.DONE)
            {

                // get the end index of the token c belongs to
                int tokenEnd = input.getRunLimit(CLASS_ANNO);

                // get the start index of the token
                int tokenStart = input.Index;
                // set iterator to next non-whitespace token
                c = input.setIndex(input.findNextAnnotation(CLASS_ANNO));

                // if the next token is a period immediately following the current token,
                // we have found a candidate for an abbreviation
                if (c == '.' && tokenEnd == input.Index)
                {
                    // get the token content WITH the following period
                    tokenEnd = tokenEnd + 1;
                    string image = input.substring(tokenStart, tokenEnd - tokenStart);

                    // if the abbreviation contains a hyphen, it's sufficient to check
                    // the part after the hyphen
                    int hyphenPos = image.LastIndexOf("-", StringComparison.Ordinal);
                    if (hyphenPos != -1)
                    {
                        string afterHyphen = image.Substring(hyphenPos + 1);
                        if (afterHyphen.matches("[^0-9]{2,}"))
                        {
                            image = afterHyphen;
                        }
                    }

                    // check if token is in abbreviation lists
                    bool found = false;
                    foreach (KeyValuePair<string, ISet<string>> oneEntry in abbrevLists.SetOfKeyValuePairs())
                    {
                        string abbrevClass = oneEntry.Key;
                        ISet<string> oneList = oneEntry.Value;
                        if (oneList.Contains(image))
                        {
                            // annotate abbreviation
                            input.annotate(CLASS_ANNO, abbrevClass, tokenStart, tokenEnd);
                            // stop looking for this abbreviation
                            found = true;
                            break;
                        }
                    }
                    if (found)
                    {
                        continue;
                    }

                    // check if token is matched by abbreviation matcher
                    if (allAbbrevMatcher.matches(image))
                    {
                        string abbrevClass = IdentifyClass(image, allAbbrevMatcher, langRes.AbbreviationDescription);
                        input.annotate(CLASS_ANNO, abbrevClass, tokenStart, tokenEnd);
                        continue;
                    }
                }
            }
        }


        /// <summary>
        /// Identifies text units and paragraphs in the given annotated string and annotates them under the
        /// annotation key BORDER_ANNO.
        /// </summary>
        /// <param name="input">
        ///          an annotated string </param>
        /// <param name="langRes">
        ///          the language resource to use </param>
        /// <exception cref="ProcessingException">
        ///              if an undefined class name is found </exception>
        private void IdentifyTus(AnnotatedString input, LanguageResource langRes)
        {

            // get matcher needed for text unit identification
            RegExp intPunctMatcher = langRes.InternalTuMatcher;

            // init end-of-sentence-mode flag; when in this mode, every token
            // that is not PTERM, PTERM_P, CLOSE_PUNCT or CLOSE_BRACKET initiates the
            // annotation of a new text unit.
            bool eosMode = false;
            bool abbrevMode = false;

            // iterate over tokens
            char c = input.setIndex(0);
            while (c != CharacterIterator.DONE)
            {

                int tokenStart = input.getRunStart(CLASS_ANNO);
                int tokenEnd = input.getRunLimit(CLASS_ANNO);
                // check if c belongs to a token
                if (null != input.getAnnotation(CLASS_ANNO))
                {
                    // check if we are in end-of-sentence mode
                    if (eosMode)
                    {
                        // if we find terminal punctuation or closing brackets,
                        // continue with the current sentence
                        if (langRes.IsAncestor(PunctDescription.TERM_PUNCT, (string)input.getAnnotation(CLASS_ANNO)) || langRes.IsAncestor(PunctDescription.TERM_PUNCT_P, (string)input.getAnnotation(CLASS_ANNO)) || langRes.IsAncestor(PunctDescription.CLOSE_PUNCT, (string)input.getAnnotation(CLASS_ANNO)) || langRes.IsAncestor(PunctDescription.CLOSE_BRACKET, (string)input.getAnnotation(CLASS_ANNO)))
                        {
                            // do nothing
                        }
                        else if (char.IsLower(c) || intPunctMatcher.matches(input.substring(input.Index, 1)))
                        {
                            // if we find a lower case letter or a punctuation that can
                            // only appear within a text unit, it was wrong alert, the
                            // sentence hasn't ended yet
                            eosMode = false;
                        }
                        else
                        {
                            // otherwise, we just found the first element of the next sentence
                            input.annotate(BORDER_ANNO, TU_BORDER, tokenStart, tokenStart + 1);
                            eosMode = false;
                        }
                    }
                    else if (abbrevMode)
                    {
                        string image = input.substring(tokenStart, tokenEnd - tokenStart);
                        if (langRes.NonCapitalizedTerms.Contains(image) || langRes.IsAncestor(PunctDescription.OPEN_PUNCT, (string)input.getAnnotation(CLASS_ANNO)))
                        {
                            // there is a term that only starts with a capital letter at the
                            // beginning of a sentence OR
                            // an opening punctuation;
                            // so we just found the first element of the next sentence
                            input.annotate(BORDER_ANNO, TU_BORDER, tokenStart, tokenStart + 1);
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
                        if (langRes.IsAncestor(PunctDescription.TERM_PUNCT, (string)input.getAnnotation(CLASS_ANNO)) || langRes.IsAncestor(PunctDescription.TERM_PUNCT_P, (string)input.getAnnotation(CLASS_ANNO)))
                        {
                            eosMode = true;
                        }
                        else if (langRes.IsAncestor(AbbrevDescription.B_ABBREVIATION, (string)input.getAnnotation(CLASS_ANNO)))
                        {
                            // check if token is a breaking abbreviation
                            abbrevMode = true;
                        }
                    }
                    // set iterator to next token
                    c = input.setIndex(tokenEnd);
                }
                else
                {
                    // check for paragraph change in whitespace sequence
                    if (this.IsParagraphChange(input.substring(tokenStart, tokenEnd - tokenStart)))
                    {
                        eosMode = false;
                        abbrevMode = false;
                        // set iterator to next token
                        c = input.setIndex(tokenEnd);
                        // next token starts a new paragraph
                        if (c != CharacterIterator.DONE)
                        {
                            input.annotate(BORDER_ANNO, P_BORDER, input.Index, input.Index + 1);
                        }
                    }
                    else
                    {
                        // just set iterator to next token
                        c = input.setIndex(tokenEnd);
                    }
                }
            }
        }


        /// <summary>
        /// Called with a sequence of whitespaces. It returns a flag indicating if the sequence contains a
        /// paragraph change. A paragraph change is defined as a sequence of whitespaces that contains two
        /// line breaks.
        /// </summary>
        /// <param name="wSpaces">
        ///          a string consisting only of whitespaces </param>
        /// <returns> a flag indicating a paragraph change </returns>
        private bool IsParagraphChange(string wSpaces)
        {

            int len = wSpaces.Length;
            for (int i = 0; i < len; i++)
            {
                char c = wSpaces[i];
                if (('\n' == c) || ('\r' == c))
                {
                    // possible continuations for a paragraph change:
                    // - another \n -> paragraph change in Unix or Windows
                    // the second \n must no be the next character!
                    // this way we catch \n\n for Unix and \r\n\r\n for Windows
                    // - another \r -> paragraph change in MacOs or Windows
                    // the second \r must no be the next character!
                    // this way we catch \r\r for MacOs and \r\n\r\n for Windows
                    // we just look for a second occurrence of the c just found
                    for (int j = i + 1; j < len; j++)
                    {
                        if (c == wSpaces[j])
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }


        /// <summary>
        /// Identifies the class of the given string and returns the corresponding class name for
        /// annotation.
        /// </summary>
        /// <param name="string">
        ///          the string for which to find the class name </param>
        /// <param name="regExp">
        ///          the regular expression that found the string as a match, {@code null} if string wasn't
        ///          found via a regular expression </param>
        /// <param name="descr">
        ///          a description that contains everything needed for identifying the class </param>
        /// <returns> the class name </returns>
        /// <exception cref="ProcessingException">
        ///              if class of string can't be identified </exception>
        private static string IdentifyClass(string @string, RegExp regExp, Description descr)
        {

            // first try to identify class via the regular expression
            if (null != regExp)
            {
                IDictionary<RegExp, string> regExpMap = descr.RegExpMap;
                string oneClass = regExpMap[regExp];
                if (null != oneClass)
                {
                    return oneClass;
                }
            }

            // get hash map with classes
            IDictionary<string, RegExp> definitionsMap = descr.DefinitionsMap;
            // iterate over classes
            foreach (KeyValuePair<string, RegExp> oneEntry in definitionsMap.SetOfKeyValuePairs())
            {
                // check if string is of that class
                string oneClass = oneEntry.Key;
                RegExp oneRe = oneEntry.Value;
                if (oneRe.matches(@string))
                {
                    // return class name
                    return oneClass;
                }
            }
            // throw exception if no class for string was found
            throw new ProcessingException(string.Format("could not find class for {0}", @string));
        }


        /// <summary>
        /// This main method must be used with two or three arguments:
        /// <ul>
        /// <li>a file name for the document to tokenize
        /// <li>the language of the document
        /// <li>an optional encoding to use (default is UTF-8)
        /// </ul>
        /// </summary>
        /// <param name="args">
        ///          the arguments </param>
        public static void Main(string[] args)
        {

            // check for correct arguments
            if ((args.Length != 2) && (args.Length != 3))
            {
                Console.Write("This method needs two arguments:%n" + "- a file name for the document to tokenize%n" + "- the language of the document%n" + "- an optional encoding to use (default is UTF-8)");
                Environment.Exit(1);
            }

            // check encoding
            string encoding = "UTF-8";
            if (args.Length == 3)
            {
                encoding = args[2];
            }

            string text = null;
            try
            {
                // get text from file
                text = FileTools.readFileAsString(new File(args[0]), encoding);
            }
            catch (IOException ioe)
            {
                Console.WriteLine(ioe.ToString());
                Console.Write(ioe.StackTrace);
                Environment.Exit(1);
            }

            try
            {
                // create new instance of JTok
                NTok testTok = new NTok();

                // tokenize text
                AnnotatedString result = testTok.Tokenize(text, args[1]);

                // print result
                foreach (Paragraph onePara in Outputter.createParagraphs(result))
                {
                    Console.WriteLine(onePara);
                }
            }
            catch (IOException e)
            {
                logger.error(e.LocalizedMessage, e);
            }
        }
    }

}