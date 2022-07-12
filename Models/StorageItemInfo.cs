namespace AzureStorageHelper.Models
{
    public class StorageItemInfo
    {
        public BinaryData BlobDownloadResult;

        // e.g. "application/pdf" or "image/png"
        public string ContentType;

        public StorageItemInfo(BinaryData blobDownloadResult, string contentType)
        {
            this.BlobDownloadResult = blobDownloadResult;
            this.ContentType = contentType;
        }
    }
}
