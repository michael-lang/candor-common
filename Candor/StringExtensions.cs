using System;
using System.Collections.Generic;
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

        //TODO: Create a separate character set class holding the characters, # of characters, and # of case insensitive unique characters.
        //  - for incrementing by large numbers, and do base X math, and with or without case sensitivity
        private static readonly char[] NumericCharacterSet = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        private static readonly char[] AsciiAlphaLowerCharacterSet = new[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };
        private static readonly char[] AsciiAlphaUpperCharacterSet = new[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
        private static readonly char[] AsciiAlphaCharacterSet = new[] { 'A', 'a', 'B', 'b', 'C', 'c', 'D', 'd', 'E', 'e', 'F', 'f', 'G', 'g', 'H', 'h', 'I', 'i', 'J', 'j', 'K', 'k', 'L', 'l', 'M', 'm', 'N', 'n', 'O', 'o', 'P', 'p', 'Q', 'q', 'R', 'r', 'S', 's', 'T', 't', 'U', 'u', 'V', 'v', 'W', 'w', 'X', 'x', 'Y', 'y', 'Z', 'z' };
        private static readonly char[] AsciiAlphaNumericLowerCharacterSet = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };
        private static readonly char[] AsciiAlphaNumericUpperCharacterSet = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
        private static readonly char[] AsciiAlphaNumericCharacterSet = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'a', 'B', 'b', 'C', 'c', 'D', 'd', 'E', 'e', 'F', 'f', 'G', 'g', 'H', 'h', 'I', 'i', 'J', 'j', 'K', 'k', 'L', 'l', 'M', 'm', 'N', 'n', 'O', 'o', 'P', 'p', 'Q', 'q', 'R', 'r', 'S', 's', 'T', 't', 'U', 'u', 'V', 'v', 'W', 'w', 'X', 'x', 'Y', 'y', 'Z', 'z' };

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
            var setCharsHasCase = false;
            var chars = source.ToCharArray().ToList();
            List<char> setChars;
            if (charSet == LexicalCharacterSet.Numeric)
            {
                if (chars.All(value => NumericCharacterSet.Contains(value) || char.IsWhiteSpace(value)))
                    setChars = NumericCharacterSet.ToList();
                else
                    throw new ArgumentException(
                        "The source string contains characters not available in the specified lexical increment character set.");
            }
            else if (charSet == LexicalCharacterSet.AsciiAlpha)
            {
                if (ignoreCase && chars.All(value => char.IsLower(value) || char.IsWhiteSpace(value)))
                    setChars = AsciiAlphaLowerCharacterSet.ToList();
                else if (ignoreCase && chars.All(value => char.IsUpper(value) || char.IsWhiteSpace(value)))
                    setChars = AsciiAlphaUpperCharacterSet.ToList();
                else if (chars.All(value => AsciiAlphaCharacterSet.Contains(value) || char.IsWhiteSpace(value)))
                {
                    setChars = AsciiAlphaCharacterSet.ToList();
                    setCharsHasCase = true;
                }
                else
                    throw new ArgumentException(
                        "The source string contains characters not available in the specified lexical increment character set.");
            }
            else if (charSet == LexicalCharacterSet.AsciiAlphaNumeric)
            {
                if (ignoreCase &&
                    chars.All(value => char.IsLower(value) || char.IsDigit(value) || char.IsWhiteSpace(value)))
                    setChars = AsciiAlphaNumericLowerCharacterSet.ToList();
                else if (ignoreCase &&
                         chars.All(value => char.IsUpper(value) || char.IsNumber(value) || char.IsWhiteSpace(value)))
                    setChars = AsciiAlphaNumericUpperCharacterSet.ToList();
                else if (chars.All(value => AsciiAlphaNumericCharacterSet.Contains(value) || char.IsWhiteSpace(value)))
                {
                    setChars = AsciiAlphaNumericCharacterSet.ToList();
                    setCharsHasCase = true;
                }
                else
                    throw new ArgumentException(
                        "The source string contains characters not available in the specified lexical increment character set.");
            }
            else //if (charSet == CharacterSet.AsciiAuto)
            {
                if (chars.All(value => NumericCharacterSet.Contains(value) || char.IsWhiteSpace(value)))
                    setChars = NumericCharacterSet.ToList();
                else if (ignoreCase &&
                         chars.All(value => AsciiAlphaLowerCharacterSet.Contains(value) || char.IsWhiteSpace(value)))
                    setChars = AsciiAlphaLowerCharacterSet.ToList();
                else if (ignoreCase &&
                         chars.All(value => AsciiAlphaUpperCharacterSet.Contains(value) || char.IsWhiteSpace(value)))
                    setChars = AsciiAlphaUpperCharacterSet.ToList();
                else if (chars.All(value => AsciiAlphaCharacterSet.Contains(value) || char.IsWhiteSpace(value)))
                {
                    setChars = AsciiAlphaCharacterSet.ToList();
                    setCharsHasCase = true;
                }
                else if (ignoreCase &&
                         chars.All(
                             value =>
                             AsciiAlphaNumericLowerCharacterSet.Contains(value) || char.IsWhiteSpace(value)))
                    setChars = AsciiAlphaNumericLowerCharacterSet.ToList();
                else if (ignoreCase &&
                         chars.All(
                             value =>
                             AsciiAlphaNumericUpperCharacterSet.Contains(value) || char.IsWhiteSpace(value)))
                    setChars = AsciiAlphaNumericUpperCharacterSet.ToList();
                else if (
                    chars.All(
                        value =>
                        AsciiAlphaNumericCharacterSet.Contains(value) || char.IsWhiteSpace(value)))
                {
                    setChars = AsciiAlphaNumericCharacterSet.ToList();
                    setCharsHasCase = true;
                }
                else
                    throw new ArgumentException(
                        "The source string contains characters not available in the specified lexical increment character set.");
            }

            //if all character positions are already at the highest character, 
            // then shortcut taking original and adding another lowest position char to end.
            if (chars.All(value => setChars.IndexOf(value) >= setChars.Count - (ignoreCase && setCharsHasCase ? 2 : 1)))
                return source + setChars[0];

            for (var c = chars.Count - 1; c >= 0; c--)
            {
                if (char.IsWhiteSpace(chars[c]))
                {
                    chars[c] = setChars[0];
                    break;
                }
                var index = setChars.IndexOf(chars[c]);
                var next = chars[c].FindNext(setChars, ignoreCase);
                var nextIndex = setChars.IndexOf(next);
                chars[c] = next;

                if (nextIndex > index)
                    break;
            }

            return String.Concat(chars);
        }
        public static char FindNext(this char after, List<char> chars, bool ignoreCase)
        {
            var index = chars.IndexOf(after);
            for (var c = index+1; c <= chars.Count-1; c++)
            {
                if (!chars[c].ToString()
                             .Equals(after.ToString(),
                                     ignoreCase
                                         ? StringComparison.InvariantCultureIgnoreCase
                                         : StringComparison.InvariantCulture))
                    return chars[c];
            }
            if (ignoreCase && char.IsLetter(after))
            {
                for (var c = 0; c < index; c++)
                {
                    if (!char.IsLetter(chars[c]))
                        return chars[c];
                    if (char.IsLower(after) && char.IsLower(chars[c]))
                        return chars[c];
                    if (char.IsUpper(after) && char.IsUpper(chars[c]))
                        return chars[c];
                }
            }
            return chars[0];
        }
    }
}
