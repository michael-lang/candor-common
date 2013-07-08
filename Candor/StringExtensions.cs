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
            var chars = source.ToCharArray().ToList();
            if (!chars.All(value => char.IsWhiteSpace(value) || charSet.Characters.Contains(value)))
                throw new ArgumentException(
                    "The source string contains characters not available in the specified lexical increment character set.");
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
                    }

                    chars[chars.Count - position] = characters[((posCharIndex + posCount) % mathBase)];
                    carryOver = posCharIndex + posCount < characters.Count - 1 ? 0 : 1;
                    remain -= posCount * (int)positionBase;
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
