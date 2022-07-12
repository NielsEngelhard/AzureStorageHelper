using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using AzureStorageHelper.Helpers;
using AzureStorageHelper.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureStorageHelper
{
    public class AzureBlobStorageService : IBlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;

        public AzureBlobStorageService(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;
        }

        #region delete storage item
        public async Task<bool> DeleteStorageItemAsync(string path)
        {
            try
            {
                path = FormatStoragePath(path);

                var blobClient = GetBlobClient(path);
                return await blobClient.DeleteIfExistsAsync();
            }
            catch
            {
                return false;
            }
        }

        public bool DeleteStorageItem(string path)
        {
            try
            {
                path = FormatStoragePath(path);

                var blobClient = GetBlobClient(path);
                return blobClient.DeleteIfExists();
            }
            catch
            {
                return false;
            }
        }
        #endregion delete storage item

        #region get raw storage item
        public async Task<StorageItemInfo> GetRawStorageItemAsync(string path)
        {
            var blobClient = GetBlobClient(path);
            var blobDownloadResult = await blobClient.DownloadContentAsync();

            return new StorageItemInfo(blobDownloadResult.Value.Content, blobDownloadResult.Value.Details.ContentType);
        }

        public StorageItemInfo GetRawStorageItem(string path)
        {
            var blobClient = GetBlobClient(path);
            var blobDownloadResult = blobClient.DownloadContent();

            return new StorageItemInfo(blobDownloadResult.Value.Content, blobDownloadResult.Value.Details.ContentType);
        }
        #endregion get raw storage item

        public Uri GetTempBlobLink(string path, int minutes = 1)
        {
            var blobClient = GetBlobClient(path);

            var currentDateTime = DateTime.Now;
            var currentDateTimePlusThreeMinutes = new DateTimeOffset(currentDateTime).ToOffset(TimeSpan.FromMinutes(minutes));

            return blobClient.GenerateSasUri(BlobSasPermissions.Read, currentDateTimePlusThreeMinutes);
        }

        #region Upload content bytes[]
        public async Task<Uri> UploadContentAsync(byte[] bytes, string path)
        {
            var blobClient = GetBlobClient(path);

            using (var stream = new MemoryStream(bytes))
            {
                var result = await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = path.GetFileOfPath().GetContentType() });
            }

            return blobClient.Uri;
        }

        public Uri UploadContent(byte[] bytes, string path)
        {
            var blobClient = GetBlobClient(path);

            using (var stream = new MemoryStream(bytes))
            {
                var result = blobClient.Upload(stream, new BlobHttpHeaders { ContentType = path.GetFileOfPath().GetContentType() });
            }

            return blobClient.Uri;
        }

        #endregion Upload content bytes[]

        #region Upload content IFormFile
        public async Task<Uri> UploadContentAsync(IFormFile file, string path)
        {
            var blobClient = GetBlobClient(path);

            using (var stream = file.OpenReadStream())
            {
                var result = await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = path.GetFileOfPath().GetContentType() });
            }

            return blobClient.Uri;
        }

        public Uri UploadContent(IFormFile file, string path)
        {
            var blobClient = GetBlobClient(path);

            using (var stream = file.OpenReadStream())
            {
                var result = blobClient.Upload(stream, new BlobHttpHeaders { ContentType = path.GetFileOfPath().GetContentType() });
            }

            return blobClient.Uri;
        }
        #endregion Upload content IFormFile

        #region Upload content Stream
        public async Task<Uri> UploadContentAsync(Stream stream, string path)
        {
            var blobClient = GetBlobClient(path);
            await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = path.GetFileOfPath().GetContentType() });

            return blobClient.Uri;
        }

        public Uri UploadContent(Stream stream, string path)
        {
            var blobClient = GetBlobClient(path);
            blobClient.Upload(stream, new BlobHttpHeaders { ContentType = path.GetFileOfPath().GetContentType() });

            return blobClient.Uri;
        }
        #endregion Upload content Stream

        /// <summary>
        /// Strips the storage path from an url (if an url is provided)
        /// 
        /// e.g. https://banger.blob.core.windows.net/playlists/playlist-photo/24f89f39-45ab-4d6f-8b8f-97bc49e50479.jpeg
        /// becomes playlists/playlist-photo/24f89f39-45ab-4d6f-8b8f-97bc49e50479.jpeg
        /// 
        /// If a valid storage path is provided, the input will not be modified and will be returned the same.
        /// </summary>
        private string FormatStoragePath(string storagePath)
        {
            if (storagePath.StartsWith("https://"))
            {
                var splittedPath = storagePath.Split(".net/");
                storagePath = splittedPath[1];
            }

            return storagePath;
        }

        private BlobClient GetBlobClient(string path)
        {
            return GetBlobClient(path.GetContainerName(), path.PathWithoutContainer());
        }

        private BlobClient GetBlobClient(string container, string blobLocation)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(container);
            return containerClient.GetBlobClient(blobLocation);
        }
    }
}
