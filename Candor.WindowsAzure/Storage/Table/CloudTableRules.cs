using System;

namespace Candor.WindowsAzure.Storage.Table
{
    public static class CloudTableRules
    {
        /// <summary>
        /// Gets a valid azure identifier for table names by trimming out 
        /// invalid characters.  If the name is still invalid, then an
        /// exception occurs.  See remarks for naming rules.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <remarks>
        /// Table names must be unique within an account.
        /// Table names may contain only alphanumeric characters.
        /// Table names cannot begin with a numeric character.
        /// Table names are case-insensitive.
        /// Table names must be from 3 to 63 characters long.
        /// Some table names are reserved, including "tables". Attempting to 
        /// create a table with a reserved table name returns error code 404 
        /// (Bad Request).
        /// http://msdn.microsoft.com/en-us/library/windowsazure/dd179338.aspx
        /// These rules are also described by the regular expression "^[A-Za-z][A-Za-z0-9]{2,62}$".
        /// Table names preserve the case with which they were created, but are case-insensitive when used.
        /// </remarks>
        public static String GetValidTableName(this String name)
        {
            var cleanupAttempt = CompiledExpressions.NonAlphaNumericReplaceRegex.Replace(name, "");
            if (!CompiledExpressions.AzureTableValidationRegex.IsMatch(cleanupAttempt))
                throw new ArgumentException("Cannot Determine a valid table name for the supplied string.");

            return cleanupAttempt;
        }
        /// <summary>
        /// Gets a valid azure identifier for partition keys.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <remarks>
        /// The following characters are not allowed in values for the PartitionKey and RowKey properties:
        /// The forward slash (/) character
        /// The backslash (\) character
        /// The number sign (#) character
        /// The question mark (?) character
        /// 
        /// http://blogs.msdn.com/b/windowsazurestorage/archive/2012/05/28/partitionkey-or-rowkey-containing-the-percent-character-causes-some-windows-azure-tables-apis-to-fail.aspx
        /// Also the % character while allowed by Azure presents problems on retrieval due to double decoding by Azure.
        /// </remarks>
        public static String GetValidPartitionKey(this String name)
        {
            return CompiledExpressions.PartitionKeyInvalidCharacterReplaceRegex.Replace(name, "");
        }
        /// <summary>
        /// Gets a valid azure identifier for row keys.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <remarks>
        /// The following characters are not allowed in values for the PartitionKey and RowKey properties:
        /// The forward slash (/) character
        /// The backslash (\) character
        /// The number sign (#) character
        /// The question mark (?) character
        /// 
        /// http://blogs.msdn.com/b/windowsazurestorage/archive/2012/05/28/partitionkey-or-rowkey-containing-the-percent-character-causes-some-windows-azure-tables-apis-to-fail.aspx
        /// Also the % character while allowed by Azure presents problems on retrieval due to double decoding by Azure.</remarks>
        public static String GetValidRowKey(this String name)
        {
            return CompiledExpressions.PartitionKeyInvalidCharacterReplaceRegex.Replace(name, "");
        }
    }
}
