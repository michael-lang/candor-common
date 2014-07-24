using System;
using System.IO;
using System.Xml.Serialization;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace Candor.WindowsAzure.Storage.Queue
{
    /// <summary>
    /// A proxy for simplifying operations against an Azure cloud queue.
    /// </summary>
    /// <typeparam name="T">The type of entity to notify about in this queue.</typeparam>
    /// <remarks>
    /// This proxy should be persisted with a long lifecycle, possible a singleton per table.
    /// A longer lifetime will result in less garbage collection of this resuable proxy type.
    /// The methods of this type are thread safe.
    /// </remarks>
    public class CloudQueueProxy<T>
        where T : class, new()
    {
        private string _connectionName;
        private String _queueName;
        private CloudStorageAccount _account;
        private CloudQueueClient _queueClient;
        private CloudQueue _queue;

        /// <summary>
        /// Gets the connection name to the Azure Queue storage.
        /// </summary>
        public string ConnectionName
        {
            get { return _connectionName; }
            set
            {
                _connectionName = value;
                _queueClient = null;
                _queue = null;
            }
        }
        /// <summary>
        /// Gets or sets the Azure Queue name.
        /// </summary>
        public String QueueName
        {
            get { return _queueName; }
            set
            {
                _queueName = value.GetValidQueueName();
                _queue = null;
            }
        }
        /// <summary>
        /// Gets the initialized CloudQueue instance given the queue name.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">The connectionName must be supplied.</exception>
        public CloudQueue GetQueue()
        {
            if (String.IsNullOrEmpty(QueueName))
                return null;

            if (_queue == null)
            {
                if (String.IsNullOrWhiteSpace(_connectionName))
                    throw new InvalidOperationException("The Cloud ConnectionName has not been configured.");
                if (_account == null)
                    _account = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting(_connectionName));
                if (_queueClient == null)
                    _queueClient = _account.CreateCloudQueueClient();
                _queue = _queueClient.GetQueueReference(!String.IsNullOrWhiteSpace(QueueName) ? QueueName : typeof(T).Name.GetValidQueueName());
            }
            return _queue;
        }
        /// <summary>
        /// Puts a new change notification on the queue.
        /// </summary>
        /// <param name="detail"></param>
        public void AddRecordChangeNotification(RecordChangeNotification detail)
        {
            using (Stream stream = new MemoryStream())
            {
                new XmlSerializer(typeof(RecordChangeNotification)).Serialize(stream, detail);
                stream.Position = 0;
                var result = new StreamReader(stream).ReadToEnd();

                var queue = GetQueue();
                queue.CreateIfNotExists();
                var message = new CloudQueueMessage(result);
                queue.AddMessage(message);
            }
        }
        /// <summary>
        /// Retrieves the change notification from the raw cloud queue message.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public RecordChangeNotification DeserializeRecordChangeNotification(CloudQueueMessage message)
        {
            using (Stream stream = new MemoryStream())
            {
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write(message.AsString);
                    writer.Flush();

                    stream.Position = 0;
                    var result = new XmlSerializer(typeof (RecordChangeNotification)).Deserialize(stream);

                    return (RecordChangeNotification) result;
                }
            }
        }
    }
}
