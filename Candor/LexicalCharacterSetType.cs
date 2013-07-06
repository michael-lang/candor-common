namespace Candor
{
    /// <summary>
    /// Character set options for Lexical String Extensions.
    /// </summary>
    public enum LexicalCharacterSetType
    {
        None = 0,
        /// <summary>
        /// Numeric only characters 0-9.
        /// </summary>
        Numeric = 1,
        /// <summary>
        /// Determine based on input string the most narrow character set to use of 'Numeric', 'AsciiAlpha', or 'AsciiAlphaNumeric'
        /// </summary>
        AsciiAuto = 2,
        /// <summary>
        /// Alpha characters only, from the ASCII character set.
        /// </summary>
        AsciiAlpha = 3,
        /// <summary>
        /// Alpha characters from the ASCII character set, plus numeric.
        /// </summary>
        AsciiAlphaNumeric = 4
    }
}