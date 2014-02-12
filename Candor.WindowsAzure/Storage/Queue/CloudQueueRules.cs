using System;

namespace Candor.WindowsAzure.Storage.Queue
{
    public static class CloudQueueRules
    {
        /// <summary>
        /// Gets a valid azure identifier for queue names by trimming out
        /// invalid characters, and changing to lower case.  If the name is
        /// still invalid, then an exception occurs.  See remarks for naming rules.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <remarks>
        /// A queue name must start with a letter or number, and can only contain
        /// letters, numbers, and the dash (-) character.
        /// The first and last letters in the queue name must be alphanumeric.  The
        /// dash (-) character cannot be the first or last character.  Consecutive
        /// dash characters are not permitted in the queue name.
        /// All letters in a queue name must be lowercase.
        /// A queue name must be from 3 to 63 characters long.
        /// </remarks>
        public static String GetValidQueueName(this String name)
        {
            var cleanupAttempt = CompiledExpressions.NonAlphaNumericReplaceRegex.Replace(name, "")
                .ToLower()
                .Replace("--", "-");
            if (!CompiledExpressions.AzureQueueValidationRegex.IsMatch(cleanupAttempt))
                throw new ArgumentException("Cannot Determine a valid queue name for the supplied string.");

            return cleanupAttempt;
        }
    }
}
