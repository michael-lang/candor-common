using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Candor.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Candor.WindowsAzure.Storage.Blob
{
    public class CloudBlobProxy<T>
        where T : class, new()
    {
        private String _connectionName;
        private String _containerName;
        private CloudStorageAccount _account;
        private CloudBlobClient _blobClient;
        private CloudBlobContainer _container;

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
                _blobClient = null;
                _container = null;
            }
        }
        /// <summary>
        /// Gets or sets the Azure container name, or leave null to use the Name of T.
        /// </summary>
        public String ContainerName
        {
            get { return _containerName; }
            set
            {
                _containerName = value.GetValidBlobContainerName();
                _container = null;
            }
        }
        public CloudBlobContainer GetContainer()
        {
            if (_container == null)
            {
                if (String.IsNullOrWhiteSpace(_connectionName))
                    throw new InvalidOperationException("The Cloud ConnectionName has not been configured.");
                if (_account == null)
                    _account = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting(_connectionName));
                if (_blobClient == null)
                    _blobClient = _account.CreateCloudBlobClient();
                _container = _blobClient.GetContainerReference(!String.IsNullOrWhiteSpace(ContainerName) ? ContainerName : typeof(T).Name.GetValidBlobContainerName());
            }
            return _container;
        }
        /// <summary>
        /// Gets or sets a function that takes T and returns the blob folder/file name.  
        /// This may also include forward slashes to designate folders and dots (.) to designate file extensions.
        /// </summary>
        public Func<T, String> BlobName { get; set; }

        public T Get(String blobName)
        {
            var container = GetContainer();
            container.CreateIfNotExists();

            var blockBlob = container.GetBlockBlobReference(blobName);
            if (!blockBlob.Exists())
                return null;
            using (var stream = blockBlob.OpenRead())
            {
                var formatter = new BinaryFormatter();
                return (T)formatter.Deserialize(stream);
            }
        }
        public void Save(T item)
        {
            var container = GetContainer();
            container.CreateIfNotExists();

            var blobName = BlobName(item);
            var blockBlob = container.GetBlockBlobReference(blobName);
            var formatter = new BinaryFormatter();
            using (var memory = new MemoryStream())
            {
                formatter.Serialize(memory, item);
                memory.Seek(0, SeekOrigin.Begin);
                blockBlob.UploadFromStream(memory);
            }
        }
        public void Delete(String blobName)
        {
            var container = GetContainer();
            container.CreateIfNotExists();

            var blockBlob = container.GetBlockBlobReference(blobName);
            blockBlob.Delete();
        }
        public void Delete(T item)
        {
            var blobName = BlobName(item);
            Delete(blobName);
        }
    }
}
