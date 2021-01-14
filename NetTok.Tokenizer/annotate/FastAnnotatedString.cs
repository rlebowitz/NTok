using System;
using System.Collections.Generic;
using System.Text;
using NetTok.Tokenizer.Exceptions;
using NetTok.Tokenizer.Utilities;

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

namespace NetTok.Tokenizer.Annotate
{
    /// <summary>
    ///     FastAnnotatedString is a fast implementation of the IAnnotatedString interface.
    ///     It reserves an array of objects and an array of booleans for each newly introduced annotation key.
    ///     This provides fast access at the cost of memory. So only introduce new annotation keys if necessary.
    ///     @author Joerg Steffen, DFKI, Robert J Lebowitz, Finaltouch IT LLC
    /// </summary>
    public class FastAnnotatedString : IAnnotatedString
    {
        // current index within the string
        private int _index;

        /// <summary>
        ///     Creates a new instance of FastAnnotatedString for the given input text.
        /// </summary>
        /// <param name="inputText">The text to annotate.</param>
        public FastAnnotatedString(string inputText)
        {
            Content = Guard.NotNull(inputText);
            Index = 0;
            Annotations = new Dictionary<string, object>();
            Borders = new Dictionary<string, bool[]>();
            CurrentKey = null;
            CurrentBorders = null;
            CurrentValues = null;
        }

        /// <summary>
        ///     The map of annotation keys to arrays of objects holding the annotation values.
        /// </summary>
        /// <remarks>
        ///     The object at a certain index in the array is the annotation value of the corresponding
        ///     character in the annotated string.
        /// </remarks>
        public IDictionary<string, object> Annotations { get; }

        public IDictionary<string, bool[]> Borders { get; }

        private string Content { get; }

        // last annotation key used
        private string CurrentKey { get; set; }

        // last value array used
        private object[] CurrentValues { get; set; }

        // last border array used
        private bool[] CurrentBorders { get; set; }

        public int Length => Content.Length;

        public int Index
        {
            get => _index;
            set
            {
                if (value < 0 || value > Content.Length)
                {
                    throw new IndexOutOfRangeException($"Invalid index {value:D}");
                }

                _index = value;
            }
        }

        public char this[int index] => index < Content.Length ? Content[index] : default;

        public char SetIndex(int index)
        {
             Index = index;
            return index < Content.Length ? Content[index] : default;
        }

        public string Substring(int start, int end) => Content[start..end];

        public void Annotate(string key, object value, int start, int end)
        {
            // check if range is legal
            if (start < 0 || end > Content.Length || start > end)
            {
                throw new ArgumentException($"Invalid substring range {start:D} - {end:D}");
            }

            if (!key.Equals(CurrentKey))
            {
                object probe = null;
                // update currents
                if (Annotations.ContainsKey(key))
                {
                    probe = Annotations[key];
                }

                if (probe == null)
                {
                    // create new arrays for this key
                    CurrentValues = new object[Content.Length];
                    CurrentBorders = new bool[Content.Length];
                    CurrentKey = key;
                    // if string is not empty, the first character is already a border
                    if (Content.Length > 0)
                    {
                        CurrentBorders[0] = true;
                    }

                    // store arrays
                    Annotations[key] = CurrentValues;
                    Borders[key] = CurrentBorders;
                }
                else
                {
                    CurrentValues = (object[]) probe;
                    CurrentBorders = Borders[key];
                    CurrentKey = key;
                }
            }

            // annotate
            for (var i = start; i < end; i++)
            {
                CurrentValues[i] = value;
                CurrentBorders[i] = false;
            }

            // set border for current annotation and the implicit next annotation (if there is one)
            CurrentBorders[start] = true;
            if (end < Content.Length)
            {
                CurrentBorders[end] = true;
            }
        }

        public object GetAnnotation(string key)
        {
            if (Index < 0 || Index >= Content.Length)
            {
                return null;
            }

            if (!key.Equals(CurrentKey))
            {
                // update currents
                var probe = Annotations[key];
                if (null != probe)
                {
                    CurrentKey = key;
                    CurrentValues = (object[]) probe;
                    CurrentBorders = Borders[key];
                }
                else
                {
                    return null;
                }
            }

            // get annotation value
            return CurrentValues[Index];
        }


        public int GetRunStart(string key)
        {
            if (!key.Equals(CurrentKey))
            {
                // update currents
                object probe = Borders[key];
                if (null != probe)
                {
                    CurrentKey = key;
                    CurrentValues = (object[]) Annotations[key];
                    CurrentBorders = (bool[]) probe;
                }
                else
                {
                    return 0;
                }
            }

            // search border
            for (var i = Index; i >= 0; i--)
            {
                if (CurrentBorders[i])
                {
                    return i;
                }
            }

            return 0;
        }


        public int GetRunLimit(string key)
        {
            if (!key.Equals(CurrentKey))
            {
                // update currents
                object probe = Borders[key];
                if (null != probe)
                {
                    CurrentKey = key;
                    CurrentValues = (object[]) Annotations[key];
                    CurrentBorders = (bool[]) probe;
                }
                else
                {
                    return Content.Length;
                }
            }

            // search border
            for (var i = Index + 1; i < Content.Length; i++)
            {
                if (CurrentBorders[i])
                {
                    return i;
                }
            }

            return Content.Length;
        }

        public virtual int FindNextAnnotation(string key)
        {
            if (!key.Equals(CurrentKey))
            {
                // update currents
                var probe = Annotations[key];
                if (null != probe)
                {
                    CurrentKey = key;
                    CurrentValues = (object[]) probe;
                    CurrentBorders = Borders[key];
                }
                else
                {
                    return Content.Length;
                }
            }

            // search next annotation
            int i;
            for (i = Index + 1; i < Content.Length; i++)
            {
                if (CurrentBorders[i])
                {
                    for (var j = i; j < Content.Length; j++)
                    {
                        if (null != CurrentValues[j])
                        {
                            return j;
                        }
                    }

                    return Content.Length;
                }
            }

            return Content.Length;
        }

        public string ToString(string key)
        {
            // init result
            var result = new StringBuilder();
            // make a backup of the current index
            var backUp = Index;
            // iterate over string
            Index = 0;
            while (Index < Content.Length)
            {
                var endAnnotation = GetRunLimit(key);
                if (null != GetAnnotation(key))
                {
                    result.Append(
                        $"{Substring(Index, endAnnotation)}\t{Index}-{endAnnotation}\t{GetAnnotation(key)}{Environment.NewLine}");
                }

                Index = endAnnotation;
            }

            // restore index
            Index = backUp;
            // return result
            return result.ToString();
        }

        public override string ToString()
        {
            return new string(Content);
        }

        public object Clone()
        {
            try
            {
                var other = (FastAnnotatedString) MemberwiseClone();
                return other;
            }
            catch (Exception ex)
            {
                throw new ProcessingException(ex.Message, ex);
            }
        }
    }
}