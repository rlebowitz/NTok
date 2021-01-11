using System;
using System.Collections.Generic;
using System.Linq;

namespace NetTok.Tokenizer.Utilities
{
    public static class Guard
    {
        public static T NotNull<T>(T item) where T : class
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            return item;
        }

        public static string NotNullOrWhitespace(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
                throw new ArgumentNullException(nameof(data));
            }

            return data;
        }

        public static IEnumerable<T> NotNullOrEmpty<T>(IEnumerable<T> collection, string exception) where T : class
        {
            if (string.IsNullOrEmpty(exception))
            {
                throw new ArgumentNullException(nameof(exception));
            }

            var notNullOrEmpty = collection as T[] ?? collection.ToArray();
            if (collection == null || !notNullOrEmpty.Any())
            {
                throw new InvalidOperationException(exception);
            }

            return notNullOrEmpty;
        }

        public static List<T> NotNullOrEmpty<T>(List<T> collection, string exception) where T : class
        {
            if (string.IsNullOrEmpty(exception))
            {
                throw new ArgumentNullException(nameof(exception));
            }

            if (collection == null || collection.Count == 0)
            {
                throw new InvalidOperationException(exception);
            }

            return collection;
        }
    }
}