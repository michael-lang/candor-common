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

        /// <summary>
        /// Performs a lexicographical addition to a string by any amount.  See
        /// http://wikipedia.org/wiki/Lexicographical_order, and further
        /// remarks for this member.
        /// </summary>
        /// <example>"code" + 2 in Ascii alpha (case insensitive) == "codg"</example>
        /// <example>"code" + 2 in Ascii alpha (case sensitive) == "codf"</example>
        /// <param name="source">The string to increment from.</param>
        /// <param name="charSet">The character set defining the characters and their order.</param>
        /// <param name="ignoreCase">Specifies if case should be ignored as an incremented value.
        /// If true, incremented character positions will be the case of the majority of other
        /// values; which may or may not be the same as the character being replaced.</param>
        /// <param name="count"></param>
        /// <returns></returns>
        /// <remarks>
        /// <p>
        /// This may or may not result in a value that sorts in the correct 
        /// order as if it were a file name in a file explorer.
        /// </p>
        /// <p>
        /// If the string is at the highest character for each position or the 
        /// number added moves past that position, then a new character position
        /// is incremented to the left (by adding a character position).  This
        /// then behaves the same as if the source was left whitespace padded.
        /// Performance scales based on the number of characters incremented
        /// in the string.
        /// </p>
        /// <p>
        /// Incrementing the value always starts on the right and moves left
        /// as with numeric additions.  Right whitespace padded strings will
        /// increment values in the whitespace before advancing to the characters
        /// on the left.  If this is not desired behavior then Trim the source
        /// when passed into this method.
        /// </p>
        /// </remarks>
        public static String LexicalAdd(this String source, LexicalCharacterSet charSet, Boolean ignoreCase, Int32 count)
        {
            return LexicalAdd(source, charSet, ignoreCase, false, count);
        }

        /// <summary>
        /// Performs a lexicographical addition to a string by any amount.  See
        /// http://wikipedia.org/wiki/Lexicographical_order, and further
        /// remarks for this member.
        /// </summary>
        /// <example>"code" + 2 in Ascii alpha (case insensitive) == "codg"</example>
        /// <example>"code" + 2 in Ascii alpha (case sensitive) == "codf"</example>
        /// <param name="source">The string to increment from.</param>
        /// <param name="charSet">The character set defining the characters and their order.</param>
        /// <param name="ignoreCase">Specifies if case should be ignored as an incremented value.
        /// If true, incremented character positions will be the case of the majority of other
        /// values; which may or may not be the same as the character being replaced.</param>
        /// <param name="treatNonCharsAsSpace">
        /// Indicates if non character set characters should be treated as 
        /// a space and be eligible for incrementing.
        /// </param>
        /// <param name="count"></param>
        /// <returns></returns>
        /// <remarks>
        /// <p>
        /// This may or may not result in a value that sorts in the correct 
        /// order as if it were a file name in a file explorer.
        /// </p>
        /// <p>
        /// If the string is at the highest character for each position or the 
        /// number added moves past that position, then a new character position
        /// is incremented to the left (by adding a character position).  This
        /// then behaves the same as if the source was left whitespace padded.
        /// Performance scales based on the number of characters incremented
        /// in the string.
        /// </p>
        /// <p>
        /// Incrementing the value always starts on the right and moves left
        /// as with numeric additions.  Right whitespace padded strings will
        /// increment values in the whitespace before advancing to the characters
        /// on the left.  If this is not desired behavior then Trim the source
        /// when passed into this method.
        /// </p>
        /// </remarks>
        public static String LexicalAdd(this String source, LexicalCharacterSet charSet, Boolean ignoreCase, Boolean treatNonCharsAsSpace, Int32 count)
        {
            var chars = source.ToCharArray().ToList();
            //if (!chars.All(value => char.IsWhiteSpace(value) || charSet.Characters.Contains(value)))
            //    throw new ArgumentException(
            //        "The source string contains characters not available in the specified lexical increment character set.");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", count, "Only positive numbers can be added lexically at this time.");

            var characters = charSet.Characters;
            if (ignoreCase && charSet.IsCaseSensitive)
            {   //change 'characters' to an upper or lower case version
                var lowerCount = chars.Where(char.IsLower).Count();
                var upperCount = chars.Where(char.IsUpper).Count();
                if (lowerCount > upperCount)
                    characters = charSet.Characters.Where(value => !char.IsLetter(value) || char.IsLower(value)).ToList();
                else
                    characters = charSet.Characters.Where(value => !char.IsLetter(value) || char.IsUpper(value)).ToList();
            }
            var mathBase = ignoreCase ? charSet.CaseInsensitiveLength : charSet.CaseSensitiveLength;
            Int32 remain = count, carryOver = 0;
            //position is counting from the right most character, since we are treating these characters as a number
            for (var position = 1; remain > 0 || carryOver > 0 ; position++)
            {
                if (chars.Count < position)
                {
                    chars.Insert(0, ' ');
                }
                var positionBase = position == 1 ? 1 : Math.Pow(mathBase, position - 1);
                var posCount = ((int)((remain / positionBase) % mathBase)) + carryOver;
                if (posCount == mathBase)
                {   //no character change, pass along carry over
                    remain -= ((posCount - carryOver) * (int)positionBase);
                    carryOver = 1;
                }
                else if (posCount > 0)
                {
                    var posChar = chars[chars.Count - position];
                    var posCharIndex = characters.IndexOf(posChar);
                    if (ignoreCase && posCharIndex == -1 && char.IsLetter(posChar))
                    {   //Character removed when changing to an upper or lower case version, so get the equivalent case-insensitive character
                        posChar = char.IsLower(posChar) ? char.ToUpper(posChar) : char.ToLower(posChar);
                        posCharIndex = characters.IndexOf(posChar);
                    } //if whitespace char, leave posCharIndex at -1 for replacement
                    if (posCharIndex == -1 && !char.IsWhiteSpace(posChar) && !treatNonCharsAsSpace)
                        throw new ArgumentException(
                            "The source string contains characters not available in the specified lexical increment character set.");

                    chars[chars.Count - position] = characters[((posCharIndex + posCount) % mathBase)];
                    carryOver = posCharIndex + posCount < characters.Count ? 0 : 1;
                    remain = Math.Max(0, remain - posCount * (int)positionBase);
                }
            }

            return String.Concat(chars);
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
            return LexicalAdd(source, charSet, ignoreCase, 1);
        }
        /// <summary>
        /// Trims any characters from the end of a string that is not in the supplied list.
        /// </summary>
        /// <param name="text">The text to be scanned.</param>
        /// <param name="chars">The characters to be kept, such as a LexicalCharacterSet characters list.</param>
        /// <returns>
        /// Returns a new string with ending characters not in the list removed.  
        /// If none of the characters in  the string are in the list, then an empty string is returned.
        /// </returns>
        public static string TrimEndNotIn(this string text, IList<char> chars)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text == null ? null : string.Empty;

            while (text.Length > 1 && !chars.Contains(text[text.Length - 1]))
            {
                text = text.Substring(0, text.Length - 1);
            }
            if (text.Length == 1 && !chars.Contains(text[0]))
                return string.Empty;
            return text;
        }
        /// <summary>
        /// Trims any characters from the start of a string that is not in the supplied list.
        /// </summary>
        /// <param name="text">The text to be scanned.</param>
        /// <param name="chars">The characters to be kept, such as a LexicalCharacterSet characters list.</param>
        /// <returns>
        /// Returns a new string with starting characters not in the list removed.  
        /// If none of the characters in  the string are in the list, then an empty string is returned.
        /// </returns>
        public static string TrimStartNotIn(this string text, IList<char> chars)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text == null ? null : string.Empty;

            while (text.Length > 1 && !chars.Contains(text[0]))
            {
                text = text.Substring(1);
            }
            if (text.Length == 1 && !chars.Contains(text[0]))
                return string.Empty;
            return text;
        }
        /// <summary>
        /// Trims any characters from the start and end of a string that is not in the supplied list.
        /// </summary>
        /// <param name="text">The text to be scanned.</param>
        /// <param name="chars">The characters to be kept, such as a LexicalCharacterSet characters list.</param>
        /// <returns>
        /// Returns a new string with starting and ending characters not in the list removed.  
        /// If none of the characters in  the string are in the list, then an empty string is returned.
        /// </returns>
        public static string TrimNotIn(this string text, IList<char> chars)
        {
            return text.TrimEndNotIn(chars).TrimStartNotIn(chars);
        }
        /// <summary>
        /// Replaces all characters in a text string that are not in a given list.
        /// </summary>
        /// <param name="text">The text to be scanned.</param>
        /// <param name="chars">The characters to be kept, such as a LexicalCharacterSet characters list.</param>
        /// <param name="replacement">The string to put in place of each character not in 'chars'.</param>
        /// <returns>Returns a new string with the replaced characters.  If none of the characters in
        /// the string are in the list, then an empty string is returned.</returns>
        public static string ReplaceNotIn(this string text, IList<char> chars, String replacement)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text == null ? null : string.Empty;
            
            var newText = new StringBuilder(text.Length * replacement.Length);
            for (var i = 0; i < text.Length; i++)
            {
                if (chars.Contains(text[i]))
                    newText.Append(text[i]);
                else
                    newText.Append(replacement);
            }
            return newText.ToString();
        }
    }
}
