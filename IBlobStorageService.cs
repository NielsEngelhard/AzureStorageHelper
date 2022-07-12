﻿using AzureStorageHelper.Models;
using Microsoft.AspNetCore.Http;

namespace AzureStorageHelper
{
    /// <summary>
    /// Interface for performing actions regarding the Azure Blob Storage. 
    /// Goal: Simplify the use of Azure Blob Storage - Write less do more
    /// 
    /// This interface uses the "path" keyword a lot. The path is the whole path to the file. The path includes the container name.
    /// Example {container-name}/img/profile-pictures/private-accounts/myitem.jpeg
    /// 
    /// </summary>
    public interface IBlobStorageService
    {
        /// <summary>
        /// Delete a storage item based on the location of the file
        /// <example>
        /// For example:
        /// <code>
        /// var path = "/images/profile-picture/083E71C3-8CC9-4A05-A9F7-C047D41F1308.png";
        /// var removed = DeleteStorageItem(path);
        /// </code>
        /// deletes the "item with name 083E71C3-8CC9-4A05-A9F7-C047D41F1308.png" from the "images container" in the directory "profile pictures"
        /// </example>
        /// </summary>
        public Task<bool> DeleteStorageItemAsync(string path);
        public bool DeleteStorageItem(string path);

        /// <summary>
        /// Get a temporary url that is accassible for an x amount of minutes (standard 1 minute).
        /// 
        /// This way private blobs can be made accasible via a url without a lot of internet traffic.
        /// The chances of someone "guessing" this url are extremely little, because the link consists of a lot of characters.
        /// 
        /// Private blobs can be retrieved "raw" too, but that way the traffic will increase significally.
        /// </summary>
        public Uri GetTempBlobLink(string path, int minutes = 1);

        /// <summary>
        /// Upload content to the blob storage. 
        /// Content can be uploaded as a byte array, IFormFile or Stream.
        /// </summary>
        public Task<Uri> UploadContentAsync(byte[] bytes, string path);
        public Task<Uri> UploadContentAsync(IFormFile file, string path);
        public Task<Uri> UploadContentAsync(Stream stream, string path);

        public Uri UploadContent(byte[] bytes, string path);
        public Uri UploadContent(IFormFile file, string path);
        public Uri UploadContent(Stream stream, string path);

        /// <summary>
        /// Retrieve a storage item from the blob storage.
        /// Item will be retrieved as BinaryData. BinaryData can (for example) be converted to a Base64 string.
        /// </summary>
        public StorageItemInfo GetRawStorageItem(string path);
        public Task<StorageItemInfo> GetRawStorageItemAsync(string path);
    }
}
