using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Candor.WindowsAzure
{
    /// <summary>
    /// Commonly used compiled regular expressions.
    /// </summary>
    internal static class CompiledExpressions
    {
        /// <summary>
        /// A replacement expression to filter out non alpha numeric characters.
        /// </summary>
        public static readonly Regex NonAlphaNumericReplaceRegex = new Regex("[^a-zA-Z0-9]", RegexOptions.Compiled);
        /// <summary>
        /// A replacement expression to filter out duplicate series of forward slashes0 and dots.
        /// </summary>
        public static readonly Regex RepeatingSlashDotReplaceRegex = new Regex(@"(?=.{2,})(/[\./]+)", RegexOptions.Compiled);
        /// <summary> 
        /// A replacement expression to filter out duplicate series of dots and forward slashes.
        /// </summary>
        public static readonly Regex RepeatingDotSlashReplaceRegex = new Regex(@"(?=.{2,})(\.[\./]+)", RegexOptions.Compiled);
        /// <summary>
        /// A replacement expression to filter out non alpha numeric and dash characters.
        /// </summary>
        public static readonly Regex NonAlphaNumericDashReplaceRegex = new Regex(@"[^a-zA-Z\-0-9]", RegexOptions.Compiled);
        public static readonly Regex PartitionKeyInvalidCharacterReplaceRegex = new Regex(@"[\#\?\/\%\\]", RegexOptions.Compiled);
        /// <summary>
        /// A validation expressions for Azure Table Storage table names.
        /// </summary>
        public static readonly Regex AzureTableValidationRegex = new Regex("^[A-Za-z][A-Za-z0-9]{2,62}$", RegexOptions.Compiled);
        /// <summary>
        /// A validation expressions for Azure Queue Storage queue names.
        /// </summary>
        public static readonly Regex AzureQueueValidationRegex = new Regex(@"^(?=.{3,63}$)[a-z0-9](-?[a-z0-9])+$", RegexOptions.Singleline | RegexOptions.Compiled);
        /// <summary>
        /// A validation expressions for Azure Blob Storage container names.
        /// </summary>
        public static readonly Regex AzureBlobContainerNameValidationRegex = new Regex(@"^(?=.{3,63}$)[a-z0-9](-?[a-z0-9])+$", RegexOptions.Singleline | RegexOptions.Compiled);
        /// <summary>
        /// A validation expressions for Azure Blob Storage blob names.
        /// </summary>
        public static readonly Regex AzureBlobNameValidationRegex = new Regex(@"^(?=.{1,1024}$)[a-z0-9]([\./]?[_a-z0-9])+$", RegexOptions.Singleline | RegexOptions.Compiled);
    }
}
