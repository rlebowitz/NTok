namespace NetTok.Tokenizer
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
    }
}