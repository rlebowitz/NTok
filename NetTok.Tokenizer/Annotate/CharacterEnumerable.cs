using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace NetTok.Tokenizer.Annotate
{
    public class CharacterEnumerable : IEnumerable<char>
    {
        private List<char> CharacterList { get; } = new List<char>();

        public char this[int index]
        {
            get => CharacterList[index];
            set => CharacterList.Insert(index, value);
        }

        public IEnumerator<char> GetEnumerator()
        {
            return CharacterList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

    }
}
