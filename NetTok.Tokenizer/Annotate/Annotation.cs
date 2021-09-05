using System;
using System.Collections.Generic;
using System.Text;
using NetTok.Tokenizer.Output;

namespace NetTok.Tokenizer.Annotate
{
    public class Annotation
    {
        public int Start { get; set; }
        public int End { get; set; }

        public bool IsToken { get; set; }

        public bool IsParagraph { get; set; }

        public bool IsAbbreviation { get; set; }

        public bool IsTextUnit { get; set; }
    }
}
