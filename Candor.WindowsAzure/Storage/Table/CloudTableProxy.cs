using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Candor.WindowsAzure.Storage.Table
{
    public class CloudTableProxy<T>
        where T : class, new()
    {
        private String _connectionName;
        private String _tableName;
        private CloudStorageAccount _account;
        private CloudTableClient _tableClient;
        private CloudTable _table;

        /// <summary>
        /// Gets the connection name to the Azure Table storage.
        /// </summary>
        public string ConnectionName
        {
            get { return _connectionName; }
            set
            {
                _connectionName = value;
                _account = null;
                _tableClient = null;
                _table = null;
            }
        }
        /// <summary>
        /// Gets or sets the Azure table name, or leave null to use the Name of T.
        /// </summary>
        public String TableName
        {
            get { return _tableName; }
            set
            {
                _tableName = value.GetValidTableName();
                _table = null;
            }
        }
        public CloudTable GetTable()
        {
            if (_table == null)
            {
                if (String.IsNullOrWhiteSpace(_connectionName))
                    throw new InvalidOperationException("The Cloud ConnectionName has not been configured.");
                if (_account == null)
                    _account = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting(_connectionName));
                if (_tableClient == null)
                    _tableClient = _account.CreateCloudTableClient();
                _table = _tableClient.GetTableReference(!String.IsNullOrWhiteSpace(TableName) ? TableName : typeof(T).Name.GetValidTableName());
            }
            return _table;
        }
        /// <summary>
        /// Gets or sets a function that takes T and returns the partition key.
        /// </summary>
        public Func<T, String> PartitionKey { get; set; }
        /// <summary>
        /// Gets or sets a function that takes T and returns the row key.
        /// </summary>
        public Func<T, String> RowKey { get; set; }

        /// <summary>
        /// This sets the partition and row key to lookup by a GUID.  This 
        /// should only be used by entity tables that have no other possible 
        /// unique value combinations.
        /// </summary>
        /// <param name="guid">Enter a lambda expression to the Guid property.  ie.  x => x.RecordId </param>
        public void SetPartitionRowKeyAsGuid(Func<T, Guid> guid)
        {
            PartitionKey = g => GetGuidSingleResultPartitionKey(guid(g));
            RowKey = g => guid(g).ToString().GetValidRowKey();
        }
        private static String GetGuidSingleResultPartitionKey(Guid guid)
        {
            return guid.ToString().Split('-')[1].GetValidPartitionKey();
        }

        /// <summary>
        /// Gets a single record by unique id.  Use this only for scenarios where there is no better partition key / row key combination option,
        /// and there is only a single record for a Guid value.
        /// </summary>
        /// <param name="uniqueIdentifier"></param>
        /// <returns></returns>
        public TableEntityProxy<T> Get(Guid uniqueIdentifier)
        {
            return Get(GetGuidSingleResultPartitionKey(uniqueIdentifier),
                uniqueIdentifier.ToString().GetValidRowKey());
        }
        /// <summary>
        /// Gets a single record in a specific partition and a specific row key.
        /// </summary>
        /// <param name="partitionKey">The unique partition key.</param>
        /// <param name="rowKey">The unique row key within the partition.</param>
        /// <returns></returns>
        public TableEntityProxy<T> Get(String partitionKey, String rowKey)
        {
            var table = GetTable();
            if (!table.Exists())
                return null;

            var retrieveOperation = TableOperation.Retrieve<TableEntityProxy<T>>(partitionKey, rowKey);
            var retrievedResult = table.Execute(retrieveOperation);

            return retrievedResult.Result != null ? ((TableEntityProxy<T>)retrievedResult.Result) : null;
        }

        /// <summary>
        /// Gets all the items in a partition within a specific page of results.
        /// </summary>
        /// <param name="partitionKey">The unique partition key.</param>
        /// <param name="skipUpToRowKey">Skip all row keys up to and including this value.  Pass null to not skip any.</param>
        /// <param name="take">The number of records to take, maximum.</param>
        /// <returns></returns>
        public List<TableEntityProxy<T>> GetPartition(String partitionKey, String skipUpToRowKey = null, Int32 take = 1000)
        {
            var table = GetTable();
            if (!table.Exists())
                return null;

            var filter = TableQuery.GenerateFilterCondition(TableConstants.PartitionKey, QueryComparisons.Equal,
                                                            partitionKey);
            if (skipUpToRowKey != null)
                filter = TableQuery.CombineFilters(filter, TableOperators.And,
                                                   TableQuery.GenerateFilterCondition(TableConstants.RowKey,
                                                                                      QueryComparisons.GreaterThan,
                                                                                      skipUpToRowKey));

            TableQuery<TableEntityProxy<T>> query = new TableQuery<TableEntityProxy<T>>()
                .Where(filter)
                .Take(take);

            return table.ExecuteQuery(query).ToList();
        }
        public IEnumerable<DynamicTableEntity> GetPartitionForDelete(String partitionKey, Int32 take)
        {
            var table = GetTable();
            if (!table.Exists())
                return null;

            var projectionQuery = new TableQuery<DynamicTableEntity>()
                .Where(TableQuery.GenerateFilterCondition(TableConstants.PartitionKey, QueryComparisons.Equal, partitionKey))
                .Select(new[] { TableConstants.PartitionKey, TableConstants.RowKey, TableConstants.ETag, TableConstants.Timestamp })
                .Take(take);

            EntityResolver<DynamicTableEntity> resolver = (pk, rk, ts, props, etag) =>
                new DynamicTableEntity(pk, rk, etag, props) { Timestamp = ts };

            return table.ExecuteQuery(projectionQuery, resolver);
        }

        /// <summary>
        /// Makes a more advanced query within a partition, such as when you don't know a specific row key.
        /// </summary>
        /// <param name="partitionKey">The partition key value.</param>
        /// <param name="otherFilters">Any other filters, or null.  Use <see cref="Microsoft.WindowsAzure.Storage.Table.TableQuery"/> to generate these filters.</param>
        /// <param name="skipUpToRowKey">Skip all rows through this row key.</param>
        /// <param name="take">The number of records to take, maximum.</param>
        /// <returns></returns>
        public List<TableEntityProxy<T>> QueryPartition(string partitionKey, String otherFilters = null,
                                                          String skipUpToRowKey = null, Int32 take = 1000)
        {
            var table = GetTable();
            if (!table.Exists())
                return null;

            var filter = TableQuery.GenerateFilterCondition(TableConstants.PartitionKey, QueryComparisons.Equal, partitionKey);
            if (skipUpToRowKey != null)
            {
                filter = TableQuery.CombineFilters(filter, TableOperators.And,
                    TableQuery.GenerateFilterCondition(TableConstants.RowKey, QueryComparisons.GreaterThan, skipUpToRowKey));
            }
            if (!String.IsNullOrEmpty(otherFilters))
                filter = TableQuery.CombineFilters(filter, TableOperators.And, otherFilters);

            var query = new TableQuery<TableEntityProxy<T>>()
                .Where(filter)
                .Take(take);

            return table.ExecuteQuery(query).ToList();
        }
        /// <summary>
        /// Queries a specific partition and a specific range of partition keys (inclusive).
        /// </summary>
        /// <param name="partitionKey">The partition key to match exactly.</param>
        /// <param name="rowKeyStartRange">The inclusive start range of row keys to match.</param>
        /// <param name="rowKeyEndRange">The inclusive end range of row keys to match.</param>
        /// <param name="otherFilters">Any other filters, or null.  Use <see cref="Microsoft.WindowsAzure.Storage.Table.TableQuery"/> to generate these filters.</param>
        /// <param name="take">The number of records to take, maximum.</param>
        /// <returns></returns>
        public List<TableEntityProxy<T>> QueryPartitionRowKeyRange(String partitionKey, String rowKeyStartRange,
                                                                String rowKeyEndRange, String otherFilters = null, Int32 take = 1000)
        {
            var table = GetTable();
            if (!table.Exists())
                return null;

            var filter = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition(TableConstants.PartitionKey, QueryComparisons.Equal, partitionKey),
                TableOperators.And,
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition(TableConstants.RowKey, QueryComparisons.GreaterThanOrEqual, rowKeyStartRange),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition(TableConstants.RowKey, QueryComparisons.LessThanOrEqual, rowKeyEndRange)
                    ));
            if (!String.IsNullOrEmpty(otherFilters))
                filter = TableQuery.CombineFilters(filter, TableOperators.And, otherFilters);

            var query = new TableQuery<TableEntityProxy<T>>()
                .Where(filter)
                .Take(take);

            return table.ExecuteQuery(query).ToList();
        }
        /// <summary>
        /// Meant for a query for a two part partition key and row key, with the goal to filter on the first part
        /// of the partition key and the first part of the row key.  This can also be used to filter on the entire
        /// partition key and the first part of a row key.
        /// </summary>
        /// <param name="partitionPrefix">A value the partition key must begin with, or the full partition key.</param>
        /// <param name="rowKeyStartRange">The first whole or partial row key to match.</param>
        /// <param name="rowKeyEndRange">The final whole or partial row key to match.  All row keys that start with this will also be matched.</param>
        /// <param name="otherFilters">Any other filters, or null.  Use <see cref="Microsoft.WindowsAzure.Storage.Table.TableQuery"/> to generate these filters.</param>
        /// <param name="take">The number of records to take, maximum.</param>
        /// <returns></returns>
        public List<TableEntityProxy<T>> QueryPartitionAndRowsByPrefex(String partitionPrefix, String rowKeyStartRange,
                                                                String rowKeyEndRange, String otherFilters = null, Int32 take = 1000)
        {
            var table = GetTable();
            if (!table.Exists())
                return null;

            var filter = TableQuery.CombineFilters(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition(TableConstants.PartitionKey, QueryComparisons.GreaterThanOrEqual,
                                                       partitionPrefix),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition(TableConstants.PartitionKey, QueryComparisons.LessThan,
                                                       partitionPrefix.LexicalIncrement(LexicalCharacterSet.AsciiAlphaNumeric,
                                                                                        true))
                    ),
                TableOperators.And,
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition(TableConstants.RowKey, QueryComparisons.GreaterThanOrEqual,
                                                       rowKeyStartRange),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition(TableConstants.RowKey, QueryComparisons.LessThan,
                                                       rowKeyEndRange.LexicalIncrement(LexicalCharacterSet.AsciiAlphaNumeric,
                                                                                        true))
                    ));
            if (!String.IsNullOrEmpty(otherFilters))
                filter = TableQuery.CombineFilters(filter, TableOperators.And, otherFilters);

            var query = new TableQuery<TableEntityProxy<T>>()
                .Where(filter)
                .Take(take);

            return table.ExecuteQuery(query).ToList();
        }

        /// <summary>
        /// Queries a range of partitions.
        /// </summary>
        /// <param name="partitionRangeStartKey">The first partition key to include.</param>
        /// <param name="partitionRangeEndKey">The last partition key to include.</param>
        /// <param name="otherFilters">Any other filters or null.  Use <see cref="Microsoft.WindowsAzure.Storage.Table.TableQuery"/> to generate these filters.</param>
        /// <param name="skipUpToRowKey">Skip all rows through this row key.</param>
        /// <param name="take">The number of records to take, maximum.</param>
        /// <returns></returns>
        public List<TableEntityProxy<T>> QueryPartitions(String partitionRangeStartKey, String partitionRangeEndKey,
                                                           String otherFilters = null, String skipUpToRowKey = null, Int32 take = 1000)
        {
            var table = GetTable();
            if (!table.Exists())
                return null;

            var filter = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition(TableConstants.PartitionKey, QueryComparisons.GreaterThanOrEqual, partitionRangeStartKey),
                TableOperators.And,
                TableQuery.GenerateFilterCondition(TableConstants.PartitionKey, QueryComparisons.LessThanOrEqual, partitionRangeEndKey)
                );
            if (skipUpToRowKey != null)
            {
                filter = TableQuery.CombineFilters(filter, TableOperators.And,
                    TableQuery.GenerateFilterCondition(TableConstants.RowKey, QueryComparisons.GreaterThan, skipUpToRowKey));
            }
            if (!String.IsNullOrEmpty(otherFilters))
                filter = TableQuery.CombineFilters(filter, TableOperators.And, otherFilters);

            var query = new TableQuery<TableEntityProxy<T>>()
                .Where(filter)
                .Take(take);

            return table.ExecuteQuery(query).ToList();
        }
        /// <summary>
        /// Inserts an entity only if a with the same PartitionKey and RowKey does not already exist.
        /// </summary>
        /// <param name="item"></param>
        public TableEntityProxy<T> Insert(T item)
        {
            var table = GetTable();
            table.CreateIfNotExists();

            var wrap = new TableEntityProxy<T>(item)
                {
                    PartitionKey = PartitionKey(item),
                    RowKey = RowKey(item)
                };
            if (String.IsNullOrEmpty(wrap.PartitionKey))
                throw new ArgumentNullException("PartitionKey", "The partition key function resulted in a null partition key.");
            if (String.IsNullOrEmpty(wrap.RowKey))
                throw new ArgumentNullException("RowKey", "The partition key function resulted in a null partition key.");
            var insertOperation = TableOperation.Insert(wrap);
            table.Execute(insertOperation);
            return wrap;
        }
        /// <summary>
        /// Insert up to 100 items as a  batch. (Azure limit per batch).
        /// Batches of any larger size are broken into smaller batches.
        /// </summary>
        /// <param name="items"></param>
        public List<TableEntityProxy<T>> InsertBatch(IEnumerable<T> items)
        {
            var table = GetTable();
            table.CreateIfNotExists();

            var batchOperation = new TableBatchOperation();
            var wrappedItems = new List<TableEntityProxy<T>>();

            Int32 batchSize = 0;
            foreach (var item in items)
            {
                if (batchSize == 100)
                {   //hit max batch size, break it into pieces.
                    table.ExecuteBatch(batchOperation);
                    batchSize = 0;
                    batchOperation = new TableBatchOperation();
                }
                var wrap = new TableEntityProxy<T>(item)
                    {
                        PartitionKey = PartitionKey(item),
                        RowKey = RowKey(item)
                    };
                if (String.IsNullOrEmpty(wrap.PartitionKey))
                    throw new ArgumentNullException("PartitionKey", "The partition key function resulted in a null partition key.");
                if (String.IsNullOrEmpty(wrap.RowKey))
                    throw new ArgumentNullException("RowKey", "The partition key function resulted in a null partition key.");
                wrappedItems.Add(wrap);
                batchOperation.Insert(wrap);
                batchSize++;
            }

            if (batchSize > 0)
                table.ExecuteBatch(batchOperation);
            return wrappedItems;
        }

        /// <summary>
        /// Updates an item on the server if it has not been updated since you last retrieved it.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="eTag">The concurrency eTag supplied when you retrieved the item.</param>
        public TableEntityProxy<T> Update(T item, Func<T, String> eTag)
        {
            var table = GetTable();
            if (!table.Exists())
                throw new InvalidOperationException(String.Format("Table '{0}' does not exist.  Update not possible.", table.Name));

            var pKey = PartitionKey(item);
            var rKey = RowKey(item);

            var retrieveOperation = TableOperation.Retrieve<TableEntityProxy<T>>(pKey, rKey);
            var retrievedResult = table.Execute(retrieveOperation);
            if (retrievedResult.Result == null)
                throw new InvalidOperationException("The item specified for update does not exist.  Try an insert instead.");

            var updateEntity = (TableEntityProxy<T>)retrievedResult.Result;
            if (updateEntity.ETag != eTag(item))
                throw new InvalidOperationException("The item was changed since you retrieved it.");

            var wrap = new TableEntityProxy<T>(item)
                {   //Putting ETag on this entity update allows it to replace the item in the table storage.
                    PartitionKey = pKey,
                    RowKey = rKey,
                    ETag = updateEntity.ETag,
                    Timestamp = updateEntity.Timestamp
                };
            if (String.IsNullOrEmpty(wrap.PartitionKey))
                throw new ArgumentNullException("PartitionKey", "The partition key function resulted in a null partition key.");
            if (String.IsNullOrEmpty(wrap.RowKey))
                throw new ArgumentNullException("RowKey", "The partition key function resulted in a null partition key.");
            if (String.IsNullOrEmpty(updateEntity.ETag))
                throw new ArgumentNullException("ETag", "An Etag is required on an entity to perform an update.");
            var updateOperation = TableOperation.Replace(wrap);
            table.Execute(updateOperation);
            return wrap;
        }
        /// <summary>
        /// Updates a wrapped item that was retrieved and edited.
        /// </summary>
        /// <param name="item"></param>
        public void Update(TableEntityProxy<T> item)
        {
            var table = GetTable();
            if (!table.Exists())
                throw new InvalidOperationException(String.Format("Table '{0}' does not exist.  Update not possible.", table.Name));

            var update = TableOperation.Replace(item);
            table.Execute(update);
        }
        /// <summary>
        /// Inserts or Updates an entity whether it is on the server or not.  Use this when you don't care about concurrency.
        /// </summary>
        /// <param name="item"></param>
        public TableEntityProxy<T> InsertOrUpdate(T item)
        {
            var table = GetTable();
            table.CreateIfNotExists();

            var pKey = PartitionKey(item);
            var rKey = RowKey(item);

            var wrap = new TableEntityProxy<T>(item)
            {
                PartitionKey = pKey,
                RowKey = rKey
            };
            if (String.IsNullOrEmpty(wrap.PartitionKey))
                throw new ArgumentNullException("PartitionKey", "The partition key function resulted in a null partition key.");
            if (String.IsNullOrEmpty(wrap.RowKey))
                throw new ArgumentNullException("RowKey", "The partition key function resulted in a null partition key.");

            var retrieveOperation = TableOperation.Retrieve<TableEntityProxy<T>>(pKey, rKey);
            var retrievedResult = table.Execute(retrieveOperation);
            if (retrievedResult.Result == null)
            {
                var insertOrReplace = TableOperation.InsertOrReplace(wrap);
                table.Execute(insertOrReplace);
                return wrap;
            }

            var updateEntity = (TableEntityProxy<T>)retrievedResult.Result;
            wrap.ETag = updateEntity.ETag;
            wrap.Timestamp = updateEntity.Timestamp;

            var updateOperation = TableOperation.InsertOrReplace(wrap);
            table.Execute(updateOperation);
            return wrap;
        }
        /// <summary>
        /// Deletes an item by it's unique combination of parition key and row key using a two step retrieve and then delete.  
        /// An exception occurs if the record does not exist.
        /// An exception occurs if the record changes during the delete (between retrieve and delete) by another process.
        /// </summary>
        /// <param name="item"></param>
        public void Delete(T item)
        {
            var table = GetTable();
            if (!table.Exists())
                throw new InvalidOperationException(String.Format("Table '{0}' does not exist.  Delete not possible.", table.Name));

            var pk = PartitionKey(item);
            var rk = RowKey(item);

            if (String.IsNullOrEmpty(pk))
                throw new ArgumentNullException("PartitionKey", "The partition key function resulted in a null partition key.");
            if (String.IsNullOrEmpty(rk))
                throw new ArgumentNullException("RowKey", "The partition key function resulted in a null partition key.");

            var retrieveOperation = TableOperation.Retrieve<TableEntityProxy<T>>(
                pk,
                rk);
            var retrievedResult = table.Execute(retrieveOperation);
            var deleteEntity = (TableEntityProxy<T>)retrievedResult.Result;

            if (deleteEntity == null) return;
            var deleteOperation = TableOperation.Delete(deleteEntity);
            var result = table.Execute(deleteOperation);
            if (result.HttpStatusCode != 200)
                throw new InvalidOperationException(String.Format("Failed to delete {0}.", table.Name));
        }
        /// <summary>
        /// Deletes an item only if it has not changed since the item was retrieved.
        /// </summary>
        /// <param name="item">An item that must have the ETag supplied from a prior retrieval.</param>
        public void Delete(TableEntityProxy<T> item)
        {
            var table = GetTable();
            if (!table.Exists())
                throw new InvalidOperationException(String.Format("Table '{0}' does not exist.  Delete not possible.", table.Name));

            var deleteOperation = TableOperation.Delete(item);
            var result = table.Execute(deleteOperation);
            if (result.HttpStatusCode != 200)
                throw new InvalidOperationException(String.Format("Failed to delete {0}.", table.Name));
        }
        /// <summary>
        /// Delete up to 100 items as a  batch. (Azure limit per batch)
        /// Batches of any larger size are broken into smaller batches.
        /// </summary>
        /// <param name="items"></param>
        public Int32 DeleteBatch(IEnumerable<ITableEntity> items)
        {
            var table = GetTable();
            if (!table.Exists())
                throw new InvalidOperationException(String.Format("Table '{0}' does not exist.  Delete not possible.", table.Name));

            var batchOperation = new TableBatchOperation();

            Int32 deleted = 0;
            Int32 batchSize = 0;
            foreach (var item in items)
            {
                if (batchSize == 100)
                {   //hit max batch size, break it into pieces.
                    table.ExecuteBatch(batchOperation);
                    batchSize = 0;
                    batchOperation = new TableBatchOperation();
                }
                batchOperation.Delete(item);
                batchSize++;
                deleted++;
            }

            if (batchSize > 0)
                table.ExecuteBatch(batchOperation);
            return deleted;
        }
        /// <summary>
        /// Update up to 100 items in a batch.  (Azure limit per batch).
        /// Batches of any larger size are broken into smaller batches.
        /// </summary>
        /// <param name="items"></param>
        public void UpdateBatch(IEnumerable<TableEntityProxy<T>> items)
        {
            var table = GetTable();
            if (!table.Exists())
                throw new InvalidOperationException(String.Format("Table '{0}' does not exist.  Update not possible.", table.Name));

            var batchOperation = new TableBatchOperation();

            Int32 batchSize = 0;
            foreach (var item in items)
            {
                if (batchSize == 100)
                {   //hit max batch size, break it into pieces.
                    table.ExecuteBatch(batchOperation);
                    batchSize = 0;
                    batchOperation = new TableBatchOperation();
                }
                item.PartitionKey = PartitionKey(item.Entity);
                item.RowKey = RowKey(item.Entity);
                if (String.IsNullOrEmpty(item.PartitionKey))
                    throw new ArgumentNullException("PartitionKey", "The partition key function resulted in a null partition key.");
                if (String.IsNullOrEmpty(item.RowKey))
                    throw new ArgumentNullException("RowKey", "The partition key function resulted in a null partition key.");
                batchOperation.Insert(item);
                batchSize++;
            }

            if (batchSize > 0)
                table.ExecuteBatch(batchOperation);
        }
        /// <summary>
        /// Inserts or Replaces up to 100 items in a batch.  (Azure limit per batch).
        /// Batches of any larger size are broken into smaller batches.
        /// </summary>
        /// <param name="items"></param>
        public void InsertOrReplaceBatch(IEnumerable<TableEntityProxy<T>> items)
        {
            var table = GetTable();
            if (!table.Exists())
                throw new InvalidOperationException(String.Format("Table '{0}' does not exist.  Update not possible.", table.Name));

            var batchOperation = new TableBatchOperation();

            Int32 batchSize = 0;
            foreach (var item in items)
            {
                if (batchSize == 100)
                {   //hit max batch size, break it into pieces.
                    table.ExecuteBatch(batchOperation);
                    batchSize = 0;
                    batchOperation = new TableBatchOperation();
                }
                item.PartitionKey = PartitionKey(item.Entity);
                item.RowKey = RowKey(item.Entity);
                if (String.IsNullOrEmpty(item.PartitionKey))
                    throw new ArgumentNullException("PartitionKey", "The partition key function resulted in a null partition key.");
                if (String.IsNullOrEmpty(item.RowKey))
                    throw new ArgumentNullException("RowKey", "The partition key function resulted in a null partition key.");
                batchOperation.InsertOrReplace(item);
                batchSize++;
            }

            if (batchSize > 0)
                table.ExecuteBatch(batchOperation);
        }
        /// <summary>
        /// Inserts or Merges up to 100 items in a batch.  (Azure limit per batch).
        /// Batches of any larger size are broken into smaller batches.
        /// </summary>
        /// <param name="items"></param>
        public void InsertOrMergeBatch(IEnumerable<TableEntityProxy<T>> items)
        {
            var table = GetTable();
            if (!table.Exists())
                throw new InvalidOperationException(String.Format("Table '{0}' does not exist.  Update not possible.", table.Name));

            var batchOperation = new TableBatchOperation();

            Int32 batchSize = 0;
            foreach (var item in items)
            {
                if (batchSize == 100)
                {   //hit max batch size, break it into pieces.
                    table.ExecuteBatch(batchOperation);
                    batchSize = 0;
                    batchOperation = new TableBatchOperation();
                }
                item.PartitionKey = PartitionKey(item.Entity);
                item.RowKey = RowKey(item.Entity);
                if (String.IsNullOrEmpty(item.PartitionKey))
                    throw new ArgumentNullException("PartitionKey", "The partition key function resulted in a null partition key.");
                if (String.IsNullOrEmpty(item.RowKey))
                    throw new ArgumentNullException("RowKey", "The partition key function resulted in a null partition key.");
                batchOperation.InsertOrMerge(item);
                batchSize++;
            }

            if (batchSize > 0)
                table.ExecuteBatch(batchOperation);
        }
    }
}