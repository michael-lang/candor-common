using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace Candor.WindowsAzure.Storage.Queue
{
    [Serializable]
    public class RecordChangeNotification
    {
        public TableOperationType OperationType { get; set; }
        public String TableName { get; set; }
        public String PartitionKey { get; set; }
        public String RowKey { get; set; }
    }
}
