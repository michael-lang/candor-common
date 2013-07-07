using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Runtime;
using System.Security;
using System.Text;

namespace Candor
{
    public class LexicalCharacterSet
    {
        private readonly IList<char> _characters;
        private readonly Int32 _caseInsensitiveLength;
        private readonly string _name;

        public IList<char> Characters
        {
            get { return _characters; }
        }
        public Int32 CaseInsensitiveLength
        {
            get { return _caseInsensitiveLength; }
        }
        public Int32 CaseSensitiveLength
        {
            get { return _characters.Count; }
        }
        public Boolean IsCaseSensitive
        {
            get { return CaseInsensitiveLength < CaseSensitiveLength; }
        }
        public String Name
        {
            get { return _name; }
        }

        public LexicalCharacterSet(String name, Int32 caseInsensitiveLength, IList<char> chars)
        {
            _name = name;
            _characters = new ReadOnlyCollection<char>(chars);
            _caseInsensitiveLength = caseInsensitiveLength;
        }
        public LexicalCharacterSet(String name, Int32 caseInsensitiveLength, IEnumerable<char> chars)
        {
            _name = name;
            _characters = new ReadOnlyCollection<char>(new List<char>(chars));
            _caseInsensitiveLength = caseInsensitiveLength;
        }

        public char FindNext(char after, bool ignoreCase)
        {
            var index = _characters.IndexOf(after);
            for (var c = index + 1; c <= _characters.Count - 1; c++)
            {
                if (!_characters[c].ToString(CultureInfo.InvariantCulture)
                             .Equals(after.ToString(CultureInfo.InvariantCulture),
                                     ignoreCase
                                         ? StringComparison.InvariantCultureIgnoreCase
                                         : StringComparison.InvariantCulture))
                    return _characters[c];
            }
            if (ignoreCase && char.IsLetter(after))
            {
                for (var c = 0; c < index; c++)
                {
                    if (!char.IsLetter(_characters[c]))
                        return _characters[c];
                    if (char.IsLower(after) && char.IsLower(_characters[c]))
                        return _characters[c];
                    if (char.IsUpper(after) && char.IsUpper(_characters[c]))
                        return _characters[c];
                }
            }
            return _characters[0];
        }

        private static LexicalCharacterSet _numericCharacterSet;
        private static LexicalCharacterSet _asciiAlphaLowerCharacterSet;
        private static LexicalCharacterSet _asciiAlphaUpperCharacterSet;
        private static LexicalCharacterSet _asciiAlphaCharacterSet;
        private static LexicalCharacterSet _asciiAlphaNumericLowerCharacterSet;
        private static LexicalCharacterSet _asciiAlphaNumericUpperCharacterSet;
        private static LexicalCharacterSet _asciiAlphaNumericCharacterSet;
        private static IList<LexicalCharacterSet> _knownCharacterSets; 

        /// <summary>
        /// Numeric only characters 0-9.
        /// </summary>
        public static LexicalCharacterSet Numeric
        {
            get
            {
                return _numericCharacterSet ??
                       (_numericCharacterSet =
                        new LexicalCharacterSet("Numeric", 10, new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' }));
            }
        }
        /// <summary>
        /// Alpha characters only, from the ASCII character set.  Lower case only.
        /// </summary>
        public static LexicalCharacterSet AsciiAlphaLower
        {
            get
            {
                return _asciiAlphaLowerCharacterSet ??
                       (_asciiAlphaLowerCharacterSet =
                        new LexicalCharacterSet("AsciiAlphaLower", 26, new[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' }));
            }
        }
        /// <summary>
        /// Alpha characters only, from the ASCII character set.  Upper case only.
        /// </summary>
        public static LexicalCharacterSet AsciiAlphaUpper
        {
            get
            {
                return _asciiAlphaUpperCharacterSet ??
                       (_asciiAlphaUpperCharacterSet =
                        new LexicalCharacterSet("AsciiAlphaUpper", 26, new[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' }));
            }
        }
        /// <summary>
        /// Alpha characters only, from the ASCII character set.
        /// </summary>
        public static LexicalCharacterSet AsciiAlpha
        {
            get
            {
                return _asciiAlphaCharacterSet ??
                       (_asciiAlphaCharacterSet =
                        new LexicalCharacterSet("AsciiAlpha", 26, new[] { 'A', 'a', 'B', 'b', 'C', 'c', 'D', 'd', 'E', 'e', 'F', 'f', 'G', 'g', 'H', 'h', 'I', 'i', 'J', 'j', 'K', 'k', 'L', 'l', 'M', 'm', 'N', 'n', 'O', 'o', 'P', 'p', 'Q', 'q', 'R', 'r', 'S', 's', 'T', 't', 'U', 'u', 'V', 'v', 'W', 'w', 'X', 'x', 'Y', 'y', 'Z', 'z' }));
            }
        }
        /// <summary>
        /// Alpha characters from the ASCII character set, plus numeric. Lower case only.
        /// </summary>
        public static LexicalCharacterSet AsciiAlphaNumericLower
        {
            get
            {
                return _asciiAlphaNumericLowerCharacterSet ??
                       (_asciiAlphaNumericLowerCharacterSet =
                        new LexicalCharacterSet("AsciiAlphaNumericLower", 36, new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' }));
            }
        }
        /// <summary>
        /// Alpha characters from the ASCII character set, plus numeric. Upper case only.
        /// </summary>
        public static LexicalCharacterSet AsciiAlphaNumericUpper
        {
            get
            {
                return _asciiAlphaNumericUpperCharacterSet ??
                       (_asciiAlphaNumericUpperCharacterSet =
                        new LexicalCharacterSet("AsciiAlphaNumericUpper", 36, new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' }));
            }
        }
        /// <summary>
        /// Alpha characters from the ASCII character set, plus numeric.
        /// </summary>
        public static LexicalCharacterSet AsciiAlphaNumeric
        {
            get
            {
                return _asciiAlphaNumericCharacterSet ??
                       (_asciiAlphaNumericCharacterSet =
                        new LexicalCharacterSet("AsciiAlphaNumeric", 36, new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'a', 'B', 'b', 'C', 'c', 'D', 'd', 'E', 'e', 'F', 'f', 'G', 'g', 'H', 'h', 'I', 'i', 'J', 'j', 'K', 'k', 'L', 'l', 'M', 'm', 'N', 'n', 'O', 'o', 'P', 'p', 'Q', 'q', 'R', 'r', 'S', 's', 'T', 't', 'U', 'u', 'V', 'v', 'W', 'w', 'X', 'x', 'Y', 'y', 'Z', 'z' }));
            }
        }
        /// <summary>
        /// Gets all the known character sets ordered by smallest size first.
        /// </summary>
        public static IList<LexicalCharacterSet> KnownCharacterSets
        {
            get
            {
                return _knownCharacterSets ??
                       (_knownCharacterSets = new ReadOnlyCollection<LexicalCharacterSet>(new List<LexicalCharacterSet>(new[]
                           {
                               Numeric,
                               AsciiAlphaUpper,
                               AsciiAlphaLower,
                               AsciiAlpha,
                               AsciiAlphaNumericUpper,
                               AsciiAlphaNumericLower,
                               AsciiAlphaNumeric
                           })));
            }
        }
    }
    public static class LexicalCharacterSetExtensions
    {
        /// <summary>
        /// Casts the enumeration value of known types into the matching character set.
        /// </summary>
        /// <returns></returns>
        public static LexicalCharacterSet ToCharacterSet(this LexicalCharacterSetType knownType)
        {
            switch (knownType)
            {
                case LexicalCharacterSetType.Numeric:
                    return LexicalCharacterSet.Numeric;
                case LexicalCharacterSetType.AsciiAlpha:
                    return LexicalCharacterSet.AsciiAlpha;
                case LexicalCharacterSetType.AsciiAlphaNumeric:
                case LexicalCharacterSetType.AsciiAuto:
                    return LexicalCharacterSet.AsciiAlphaNumeric;
                default:
                    throw new ArgumentException(String.Format("LexicalCharacterSetType '{0}' cannot be converted directly to a character set.", knownType));
            }
        }
        /// <summary>
        /// Casts the enumeration value of known types into the matching character set.
        /// </summary>
        /// <returns></returns>
        public static LexicalCharacterSet ToCharacterSet(this LexicalCharacterSetType knownType, String matching, bool ignoreCase)
        {
            var chars = matching.ToCharArray().ToList();

            if (knownType == LexicalCharacterSetType.Numeric)
            {
                if (chars.All(value => LexicalCharacterSet.Numeric.Characters.Contains(value) || char.IsWhiteSpace(value)))
                    return LexicalCharacterSet.Numeric;
                throw new ArgumentException(
                    "The source string contains characters not available in the specified lexical character set.");
            }
            if (knownType == LexicalCharacterSetType.AsciiAlpha)
            {
                if (ignoreCase && chars.All(value => LexicalCharacterSet.AsciiAlphaUpper.Characters.Contains(value) || char.IsWhiteSpace(value)))
                    return LexicalCharacterSet.AsciiAlphaUpper;
                if (ignoreCase && chars.All(value => LexicalCharacterSet.AsciiAlphaLower.Characters.Contains(value) || char.IsWhiteSpace(value)))
                    return LexicalCharacterSet.AsciiAlphaLower;
                if (chars.All(value => LexicalCharacterSet.AsciiAlpha.Characters.Contains(value) || char.IsWhiteSpace(value)))
                    return LexicalCharacterSet.AsciiAlpha;
                throw new ArgumentException(
                    "The source string contains characters not available in the specified lexical character set.");
            }
            if (knownType == LexicalCharacterSetType.AsciiAlphaNumeric)
            {
                if (ignoreCase && chars.All(value => LexicalCharacterSet.AsciiAlphaNumericUpper.Characters.Contains(value) || char.IsWhiteSpace(value)))
                    return LexicalCharacterSet.AsciiAlphaNumericUpper;
                if (ignoreCase && chars.All(value => LexicalCharacterSet.AsciiAlphaNumericLower.Characters.Contains(value) || char.IsWhiteSpace(value)))
                    return LexicalCharacterSet.AsciiAlphaNumericLower;
                if (chars.All(value => LexicalCharacterSet.AsciiAlphaNumeric.Characters.Contains(value) || char.IsWhiteSpace(value)))
                    return LexicalCharacterSet.AsciiAlphaNumeric;
                throw new ArgumentException(
                    "The source string contains characters not available in the specified lexical character set.");
            }

            foreach (var charSet in LexicalCharacterSet.KnownCharacterSets)
            {
                //only allow pick of 'upper' and 'lower' case character sets if ignore case is set
                if (!ignoreCase && !charSet.IsCaseSensitive && charSet.Characters.Any(char.IsLetter)) continue;

                if (chars.All(value => charSet.Characters.Contains(value) || char.IsWhiteSpace(value)))
                    return charSet;
            }
            throw new ArgumentException(
                "The source string contains characters not available in any known (pre-defined) lexical character set.");
        }
    }
}