using System;
using System.Linq;
using System.Text;

namespace Candor
{
    /// <summary>
    /// Various common use case extensions to <see cref="System.String"/>.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Puts spaces in a camel cased string before each capital letter and makes the capital letters lower case.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string CamelCaseToPhrase(this string text)
        {
            if (string.IsNullOrEmpty(text))
                return String.Empty;
            var newText = new StringBuilder(text.Length * 2);
            newText.Append(text[0]);
            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]))
                    newText.Append(' ');
                newText.Append(text[i]);
            }
            return newText.ToString();
        }

        public static String LexicalAdd(this String source, LexicalCharacterSet charSet, Boolean ignoreCase, Int32 count)
        {
            //TODO: improve performance...
#warning improve performance of this for large counts.
            var tmp = source;
            for (int i = 0; i < count; i++)
            {
                tmp = tmp.LexicalIncrement(charSet, ignoreCase);
            }
            return tmp;
        }
        /// <summary>
        /// Increments a source string to the next logical higher value.  Characters are incremented without altering length first.
        /// If all character positions are at the highest character, then a new lowest value character is added to the end.
        /// </summary>
        /// <param name="source">The source string.</param>
        /// <param name="charSet">The character set to use.  If the source string characters do not all fit in this set, an exception is thrown.</param>
        /// <param name="ignoreCase">Specified if case is to be ignored for alpha characters.</param>
        /// <returns>The incremented string.</returns>
        /// <remarks>
        /// </remarks>
        public static String LexicalIncrement(this String source, LexicalCharacterSet charSet, Boolean ignoreCase)
        {
            var chars = source.ToCharArray().ToList();
            if (!chars.All(value => char.IsWhiteSpace(value) || charSet.Characters.Contains(value)))
                throw new ArgumentException(
                    "The source string contains characters not available in the specified lexical increment character set.");

            //if all character positions are already at the highest character, 
            // then shortcut taking original and adding another lowest position char to end.
            if (chars.All(value => charSet.Characters.IndexOf(value) >= charSet.Characters.Count - (ignoreCase && charSet.IsCaseSensitive ? 2 : 1)))
                return source + charSet.Characters[0];

            for (var c = chars.Count - 1; c >= 0; c--)
            {
                if (char.IsWhiteSpace(chars[c]))
                {
                    chars[c] = charSet.Characters[0];
                    break;
                }
                var index = charSet.Characters.IndexOf(chars[c]);
                var next = charSet.FindNext(chars[c], ignoreCase);
                var nextIndex = charSet.Characters.IndexOf(next);
                chars[c] = next;

                if (nextIndex > index)
                    break;
            }

            return String.Concat(chars);
        }
    }
}
