using System;
using System.IO;
using System.Xml.Serialization;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace Candor.WindowsAzure.Storage.Queue
{
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
        public void AddRecordChangeNotification(RecordChangeNotification detail)
        {
            using (Stream stream = new MemoryStream())
            {
                new XmlSerializer(typeof(RecordChangeNotification)).Serialize(stream, detail);
                stream.Position = 0;
                var result = new StreamReader(stream).ReadToEnd();

                var queue = GetQueue();
                var message = new CloudQueueMessage(result);
                queue.AddMessage(message);
            }
        }
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
