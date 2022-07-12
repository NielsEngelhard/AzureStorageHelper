namespace AzureStorageHelper.Helpers
{
    public static class StoragePathHelper
    {
        private static readonly string[] InvalidPathCharacters = new[] { "//" };

        // Container name is the first value from the path
        public static string GetContainerName(this string path)
        {
            if (path.Substring(0, 1) == "/")
            {
                path = path[1..];
            }
            return path.Split("/")[0];
        }

        // Get the path directory and file (path without the name of the container/path without the first value)
        public static string PathWithoutContainer(this string path)
        {
            if (path.Substring(0, 1) == "/")
            {
                path = path[1..];
            }
            return path.Substring(path.IndexOf('/') + 1);
        }

        // Get the file of the path (the last path value)
        public static string GetFileOfPath(this string path)
        {
            var lastSlashInPathArrayPosition = path.LastIndexOf("/") + 1;
            return path.Substring(lastSlashInPathArrayPosition, (path.Length - lastSlashInPathArrayPosition));
        }

        // Validates if a path is a valid path
        public static bool IsValidPath(this string path)
        {
            if (!path.Contains("/"))
            {
                return false;
            }
            else
            {
                var lastSlashInPathArrayPosition = path.LastIndexOf("/") + 1;
                var pathWithoutFile = path.Substring(0, lastSlashInPathArrayPosition);
                if (InvalidPathCharacters.Any(c => pathWithoutFile.Contains(c)))
                {
                    return false;
                }
            }
            return true;
        }

        // e.g. FormatStoragePath("images", "profile-pictures/small", "myimage.png");
        public static string FormatStoragePath(string containerName, string? directory, string imageName)
        {
            if (string.IsNullOrEmpty(containerName) || string.IsNullOrEmpty(imageName) || imageName.EndsWith("/"))
            {
                throw new InvalidDataException();
            }

            // Check containerName
            if (containerName.StartsWith("/"))
            {
                containerName = containerName.Remove(0, 1);
            }
 
            if (containerName.EndsWith("/"))
            {
                containerName = containerName.Remove(containerName.Length - 1, 1);
            }

            // Check image
            if (imageName.StartsWith("/"))
            {
                imageName = imageName.Remove(0, 1);
            }

            // Check directory
            if (directory != null)
            {
                if (directory.StartsWith("/"))
                {
                    directory = directory.Remove(0, 1);
                }

                if (directory.EndsWith("/"))
                {
                    directory = directory.Remove(directory.Length - 1, 1);
                }
            } else
            {
                return $"{containerName}/{imageName}";
            }

            return $"{containerName}/{directory}/{imageName}";
        }
    }
}
