using Microsoft.AspNetCore.StaticFiles;

namespace AzureStorageHelper.Helpers
{
    public static class FileHelper
    {
        private static readonly FileExtensionContentTypeProvider Provider = new FileExtensionContentTypeProvider();

        // Returns the content type from the file name e.g. myimage.png -> image/png
        public static string GetContentType(this string fileName)
        {
            if (!Provider.TryGetContentType(fileName, out var contentType))
            {
                contentType = "application/octet-stream";
            }
            return contentType;
        }

        // Returns the file extension from the file name e.g. myimage.png -> png
        public static string GetFileExtension(this string fileName)
        {
            var lastDotInString = fileName.LastIndexOf(".") + 1;
            return fileName.Substring(lastDotInString, (fileName.Length - lastDotInString));
        }
    }
}
