using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Candor.WindowsAzure
{
    /// <summary>
    /// Various common use case extensions to <see cref="System.String"/> related to special needs in Azure storage keys.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Create an azure partition key in a lower case format and just alpha numeric with non-repeating dashes.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string GetSimplePartitionKey(this string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            var input = text.RemoveDiacritics();
            var sb = new StringBuilder(input.Length);
            var prevAdded = '-';
            foreach (var c in input.ToCharArray())
            {
                if (!char.IsLetter(c) && !char.IsDigit(c) && !char.IsWhiteSpace(c) && c != '-')
                    continue; //don't add to where clause of foreach for performance

                if (prevAdded == '-' && (char.IsWhiteSpace(c) || c == '-'))
                    continue;

                sb.Append(prevAdded = (char.IsWhiteSpace(c) ? '-' : char.ToLower(c)));
            }
            if (sb.Length > 0 && sb[sb.Length - 1] == '-')
                sb.Remove(sb.Length - 1, 1);

            return sb.ToString();
            //The above is equivalent to the following:
            //return text.RemoveDiacritics().ToAlphaDigitSingleSpacesOnly().Replace(' ', '-').Trim('-').ToLower();
        }
        /// <summary>
        /// Create a multi-part azure partition key from a series of strings, each becoming a simple key and each separated by a pipe.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string GetSimplePartitionKey(this IEnumerable<string> text)
        {
            var sb = new StringBuilder();
            foreach (var item in text)
            {
                if (!string.IsNullOrWhiteSpace(item))
                    sb.Append(item.GetSimplePartitionKey());
                sb.Append('|');
            }
            if (sb.Length > 0)
                sb.Remove(sb.Length - 1, 1); //remove final pipe added after last item.
            return sb.ToString();
        }
        /// <summary>
        /// Create an azure partition key in a lower case format and just alpha numeric with non-repeating dashes,
        /// incremented by lexical value.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string GetSimplePartitionNextKey(this string text)
        {
            return text.GetSimplePartitionKey().LexicalIncrement(LexicalCharacterSet.AsciiAlphaNumericLower, true);
        }
        /// <summary>
        /// Create a multi-part azure partition key from a series of strings, each becoming a simple key and each separated by a pipe.
        /// Increments the last item in the enumeration by one lexical value.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string GetSimplePartitionNextKey(this IEnumerable<string> text)
        {
            var items = text.ToList();
            var sb = new StringBuilder();
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                sb.Append(i == items.Count - 1
                    ? item.GetSimplePartitionNextKey()
                    : !string.IsNullOrWhiteSpace(item)
                        ? item.GetSimplePartitionKey()
                        : "");
                if (i < items.Count - 1)
                    sb.Append('|');
            }
            return sb.ToString();
        }
        /// <summary>
        /// Create an azure row key in a lower case format and just alpha numeric with non-repeating dashes.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string GetSimpleRowKey(this string text)
        {
            return text.GetSimplePartitionKey();
        }
        /// <summary>
        /// Create a multi-part azure row key from a series of strings, each becoming a simple key and each separated by a pipe.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string GetSimpleRowKey(this IEnumerable<string> text)
        {
            return text.GetSimplePartitionKey();
        }
        /// <summary>
        /// Create an azure row key in a lower case format and just alpha numeric with non-repeating dashes,
        /// incremented by lexical value.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string GetSimpleRowNextKey(this string text)
        {
            return text.GetSimplePartitionNextKey();
        }
        /// <summary>
        /// Create a multi-part azure row key from a series of strings, each becoming a simple key and each separated by a pipe.
        /// Increments the last item in the enumeration by one lexical value.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string GetSimpleRowNextKey(this IEnumerable<string> text)
        {
            return text.GetSimplePartitionNextKey();
        }

    }
}
