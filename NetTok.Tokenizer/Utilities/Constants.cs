namespace NetTok.Tokenizer.Utilities
{
    public static class Constants
    {
        public static class Descriptions
        {
            /// <summary>
            ///     single line in descriptions that marks the start of the lists section
            /// </summary>
            public const string ListsMarker = "LISTS:";

            /// <summary>
            ///     single line in descriptions that marks the start of the definitions section
            /// </summary>
            public const string DefinitionsMarker = "DEFINITIONS:";

            /// <summary>
            ///     single line in descriptions that marks the start of the rules section
            /// </summary>
            public const string RulesMarker = "RULES:";

            /// <summary>
            ///     attribute of a definition element that contains the regular expression
            /// </summary>
            public const string DefinitionRegularExpression = "regexp";

            /// <summary>
            ///     attribute of a definition or list element that contains the class name
            /// </summary>
            public const string DefinitionClass = "class";

            /// <summary>
            ///     attribute of a list element that point to the list file
            /// </summary>
            public const string ListFile = "file";

            /// <summary>
            ///     attribute of a list element that contains the encoding of the list file
            /// </summary>
            public const string ListEncoding = "encoding";
        }

        public static class Punctuation
        {
            /// <summary>
            ///     class name for opening punctuation
            /// </summary>
            public const string OpenPunct = "OPEN_PUNCT";

            /// <summary>
            ///     class name for closing punctuation
            /// </summary>
            public const string ClosePunct = "CLOSE_PUNCT";

            /// <summary>
            ///     class name for opening brackets
            /// </summary>
            public const string OpenBracket = "OPEN_BRACKET";

            /// <summary>
            ///     class name for closing brackets
            /// </summary>
            public const string CloseBracket = "CLOSE_BRACKET";

            /// <summary>
            ///     class name for terminal punctuation
            /// </summary>
            public const string TermPunct = "TERM_PUNCT";

            /// <summary>
            ///     class name for possible terminal punctuation
            /// </summary>
            public const string TermPunctP = "TERM_PUNCT_P";

            /// <summary>
            ///     name of the all punctuation rule
            /// </summary>
            public const string AllRule = "ALL_PUNCT_RULE";

            /// <summary>
            ///     name of the internal punctuation rule
            /// </summary>
            public const string InternalRule = "INTERNAL_PUNCT_RULE";

            /// <summary>
            ///     name of the sentence internal punctuation rule
            /// </summary>
            public const string InternalTuRule = "INTERNAL_TU_PUNCT_RULE";

            /// <summary>
            ///     class name for ambiguous open/close punctuation
            /// </summary>
            public const string OpenClosePunct = "OPENCLOSE_PUNCT";

            /// <summary>
            ///     name suffix of the resource file with the punctuation description.
            /// </summary>
            public const string PunctuationDescriptionSuffix = "punct.cfg";
        }
    }
}