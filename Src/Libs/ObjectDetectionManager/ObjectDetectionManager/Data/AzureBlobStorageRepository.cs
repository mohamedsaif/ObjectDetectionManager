using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ObjectDetectionManager.Abstractions;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;

namespace ObjectDetectionManager.Data
{
    public class AzureBlobStorageRepository : IStorageRepository
    {
        CloudStorageAccount storageAccount;
        CloudBlobClient cloudBlob;
        CloudBlobContainer blobContainer;
        CloudQueueClient queueClient;
        CloudQueue newReqQueue;
        CloudQueue callbackReqQueue;

        public AzureBlobStorageRepository(string storageName, string storageKey, string storageContainer)
        {
            storageAccount = new CloudStorageAccount(
                                    new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(
                                    storageName,
                                    storageKey), true);

            //Preparing the storage container for blobs
            cloudBlob = storageAccount.CreateCloudBlobClient();
            blobContainer = cloudBlob.GetContainerReference(storageContainer);
            blobContainer.CreateIfNotExistsAsync().Wait();
        }

        public async Task<string> CreateFile(string name, Stream stream)
        {
            CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(name);

            // Create or overwrite the file name blob with the contents of the provided stream
            await blockBlob.UploadFromStreamAsync(stream);

            return blockBlob.Uri.AbsoluteUri;
        }
    }
}
