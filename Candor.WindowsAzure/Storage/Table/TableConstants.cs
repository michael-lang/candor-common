using System;

namespace Candor.WindowsAzure.Storage.Table
{
    /// <summary>
    /// Common table related constants.  These are copies of the same 
    /// 'internal' scope but publically needed constants on 
    /// <see cref="Microsoft.WindowsAzure.Storage.Table.Protocol.TableConstants"/>
    /// </summary>
    public static class TableConstants
    {
        /// <summary>
        /// The name of the partition key property.
        /// 
        /// </summary>
        public const string PartitionKey = "PartitionKey";
        /// <summary>
        /// The name of the row key property.
        /// 
        /// </summary>
        public const string RowKey = "RowKey";
        /// <summary>
        /// The name of the Timestamp property.
        /// 
        /// </summary>
        public const string Timestamp = "Timestamp";
        /// <summary>
        /// The name of the ETag property.
        /// 
        /// </summary>
        public const string ETag = "ETag";
        /// <summary>
        /// The minimum value of a datetime supported in Azure
        /// </summary>
        public static readonly DateTime MinSupportedDateTime = new DateTime(1601, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
    }
}
