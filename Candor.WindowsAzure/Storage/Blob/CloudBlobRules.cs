using System;

namespace Candor.WindowsAzure.Storage.Blob
{
    /// <summary>
    /// Helper methods to convert a string into valid blob key values.
    /// </summary>
    public static class CloudBlobRules
    {
        /// <summary>
        /// Gets a valid azure identifier for blob container names by trimming out
        /// invalid characters, and changing to lower case.  If the name is
        /// still invalid, then an exception occurs.  See remarks for naming rules.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <remarks>
        /// A blob container name must start with a letter or number, and can only contain
        /// letters, numbers, and the dash (-) character.
        /// The first and last letters in the container name must be alphanumeric.  The
        /// dash (-) character cannot be the first or last character.  Consecutive
        /// dash characters are not permitted in the container name.
        /// All letters in a container name must be lowercase.
        /// A container name must be from 3 to 63 characters long.
        /// </remarks>
        public static String GetValidBlobContainerName(this String name)
        {
            var cleanupAttempt = CompiledExpressions.NonAlphaNumericReplaceRegex.Replace(name, "")
                .ToLower();
            if (!CompiledExpressions.AzureBlobContainerNameValidationRegex.IsMatch(cleanupAttempt))
                throw new ArgumentException("Cannot Determine a valid blob container name for the supplied string.");

            return cleanupAttempt;
        }

        /// <summary>
        /// Gets a valid blob name by removing excess slashes and dots.  If the name is still invalid,
        /// then an exception occurs.  See remarks for naming rules.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="strict">Specifies if an exception is thrown if the cleaned up name would need url encoding to be stored.</param>
        /// <returns></returns>
        /// <remarks>
        /// A blob name can contain any combination of characters, but reserved URL characters must
        /// be properly escaped.  A blob name must be at least one character long and cannot be more
        /// than 1,024 characters long.  Blob names are case-sensitive.
        /// Avoid names the end with a dot (.), o forward slash (/), or a sequence or combination
        /// of the two.
        /// </remarks>
        public static String GetValidBlobName(this String name, bool strict = false)
        {
            var cleanupAttempt = CompiledExpressions.RepeatingSlashDotReplaceRegex.Replace(
                CompiledExpressions.RepeatingDotSlashReplaceRegex.Replace(name,
                                                                          "."),
                "/");
            if (cleanupAttempt.EndsWith(".") || cleanupAttempt.EndsWith("/"))
                cleanupAttempt = cleanupAttempt.Substring(0, cleanupAttempt.Length - 1);

            if (strict && !CompiledExpressions.AzureBlobNameValidationRegex.IsMatch(cleanupAttempt))
                throw new ArgumentException("Cannot verify a valid blob name for the supplied string.");

            return cleanupAttempt;
        }
    }
}
