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

        public async Task<string> CreateFileAsync(string name, Stream fileData)
        {
            CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(name);

            // Create or overwrite the file name blob with the contents of the provided stream
            await blockBlob.UploadFromStreamAsync(fileData);

            return blockBlob.Uri.AbsoluteUri;
        }

        public async Task<string> CreateFileAsync(string name, byte[] fileData)
        {
            CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(name);

            // Create or overwrite the file name blob with the contents of the provided stream
            await blockBlob.UploadFromByteArrayAsync(fileData, 0, fileData.Length);

            return blockBlob.Uri.AbsoluteUri;
        }

        public async Task<string> CreateFileAsync(string containerName, string fileName, Stream fileData)
        {
            var workspaceContainer = cloudBlob.GetContainerReference(containerName);
            await workspaceContainer.CreateIfNotExistsAsync();

            CloudBlockBlob blockBlob = workspaceContainer.GetBlockBlobReference(fileName);

            // Create or overwrite the file name blob with the contents of the provided stream
            await blockBlob.UploadFromStreamAsync(fileData);
            return blockBlob.Uri.AbsoluteUri;
        }

        public async Task<byte[]> GetFileAsync(string fileName)
        {
            CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(fileName);
            using (var fileStream = new MemoryStream())
            {
                await blockBlob.DownloadToStreamAsync(fileStream);
                return fileStream.ToArray();
            }
        }

        public string GetFileDownloadUrl(string fileName)
        {
            CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(fileName);

            SharedAccessBlobPolicy adHocReadOnlyPolicy = new SharedAccessBlobPolicy()
            {
                // When the start time for the SAS is omitted, the start time is assumed to be the time when the storage service receives the request.
                // Omitting the start time for a SAS that is effective immediately helps to avoid clock skew.
                SharedAccessExpiryTime = DateTime.UtcNow.AddHours(1),
                Permissions = SharedAccessBlobPermissions.Read
            };

            var sas = blockBlob.GetSharedAccessSignature(adHocReadOnlyPolicy);

            return blockBlob.Uri + sas;
        }

        public async Task<byte[]> GetFileAsync(string containerName, string fileName)
        {
            var workspaceContainer = cloudBlob.GetContainerReference(containerName);
            CloudBlockBlob blockBlob = workspaceContainer.GetBlockBlobReference(fileName);
            using (var fileStream = new MemoryStream())
            {
                await blockBlob.DownloadToStreamAsync(fileStream);
                return fileStream.ToArray();
            }
        }
    }
}
