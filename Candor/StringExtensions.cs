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
            System.Text.StringBuilder newText = new System.Text.StringBuilder(text.Length * 2);
            newText.Append(text[0]);
            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]))
                    newText.Append(' ');
                newText.Append(text[i]);
            }
            return newText.ToString();
        }
    }
}
