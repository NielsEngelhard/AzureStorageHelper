﻿# NEICT-AzureStorageHelper
# _Easy use for Azure (blob) Storage_

Azure Storage Helper is an open-source lightweight package that you can include in your .NET projects. The package is meant to function as a facade ([see facade pattern](https://refactoring.guru/design-patterns/facade)) between _Your Application_ and "Azure Storage". This package provides an interface that can be used to perform all the actions you need in a simple way - with little configuration.

You can store all file types in the Azure Blob Storage (.pdf, .png, .docx, .txt, .razor, etc.)

Link to GitHub repo: https://github.com/NielsEngelhard/AzureStorageHelper
Link to Nuget: https://www.nuget.org/packages/NEICT-AzureStorageHelper

# Installation
The class library can be installed via the NuGet Package Manager:
```
Install-Package NEICT-AzureStorageHelper -Version 1.0.0
```

Or the .NET CLI:
```
dotnet add package NEICT-AzureStorageHelper --version 1.0.0
```

# Setup
This package does not work out of the box. You need to add some code to your Program.cs file. The service that is used to communicate with the storage, expects a BlobServiceClient in the constructor. This BlobServiceClient can be provided via Dependency Injection. Adding this code, will solve this:
```
// TODO: Add BlobStorageConnectionString in appsettings.json (in ConnectionStrings section)
var storageConnectionString =
    builder.Configuration.GetConnectionString("BlobStorageConnectionString");

builder.Services.AddSingleton(x =>
    new BlobServiceClient(storageConnectionString));
```

This class library provides an interface called IBlobStorageFacade. This interface can be injected in your classes (services). If you want to inject the IBlobStorageInterface in your services (and you probably want), you need to add the following line too, to the Program.cs file.
```
builder.Services.AddScoped<IAzureBlobStorageFacade, AzureBlobStorageFacade>();
```


## In short
Add the following code to your Program.cs:
```
var storageConnectionString =
    builder.Configuration.GetConnectionString("BlobStorageConnectionString");

builder.Services.AddSingleton(x =>
    new BlobServiceClient(storageConnectionString));
    
builder.Services.AddScoped<IBlobStorageFacade, AzureBlobStorageFacade>();
```

# Use example
The main functionalities of this class library are accessible via the IBlobStorageFacade (see chapter Setup on how to make this injectable). When completing the steps from the Setup, you can perform actions this way:
```
using AzureStorageHelper;

namespace Storage.Example;
public class UserService : IUserService
{
    private readonly IBlobStorageFacade _storageService;

    public UserService(IBlobStorageFacade storageService)
    {
        _storageService = storageService;
    }
    
    // Returns the location of the stored image
    public string UploadImage(IFormFile image) {
        // containerName=images path=profile-pictures/square imageName=99CAFBB2-FBC9-4CE7-9492-ADE4B859CBA7.png
        var path = "images/profile-pictures/square/99CAFBB2-FBC9-4CE7-9492-ADE4B859CBA7.png"; 
        var uri = _storageService.UploadContent(image, path);
        
        return uri.ToString();
    }
}
```

Example that uses the StoragePathHelper helper method "FormatStoragePath" to build the path.
```
using AzureStorageHelper;

namespace Storage.Example;
public class UserService : IUserService
{
    private readonly IBlobStorageFacade _storageService;

    public UserService(IBlobStorageFacade storageService)
    {
        _storageService = storageService;
    }
    
    // Returns the location of the stored image
    public string UploadImage(IFormFile image) {
        var containerName = "images";
        var path = "profile-pictures/square";
        var imageName = $"{Guid.NewGuid.ToString()}.png";

        var path = StoragePathHelper.FormatStoragePath(containerName, path, imageName); 

        var uri = _storageService.UploadContent(image, path);
        
        return uri.ToString();
    }
}
```

# Private and public blobs
Each container in the Azure Storage (Azure portal) can be made public (public read OR public write and read) or private. When a container is public, all files can be read by everyone with the link to the file. You can store this link to access the data you uploaded (when uploading a file, the uri with the location to the item is returned).

When a container is private, all the data inside of it, is not publicly accessible. See next chapter for the options of accessing this data.

Public and private items can only be specified on the container level and only in the azure portal.

## Retrieving items from a private container
When you store items in a private container, they aren't accessible via the web (with an URL). There are 2 ways of retrieving this data securely. 
1: Download the raw blob (raw content) via your application and transform this raw data to the saved file.
2: Generate a temporary link to access the file (eventually this link will expire and the blob can't be accessed via it again).

Downloading the raw data guarentees that the item is by no way accessible via the public internet (via a http link), but it is relatively slow, especially when you want to access 10 blobs at once for example. 

Interface method for downloading raw item (from IBlobStorageFacade.cs):
```
    public StorageItemInfo GetRawStorageItem(string path);
```

Generating a temporary link is in most cases the best solution. You can specify how long you want the link to be active (standard 1 minute). In that time frame, the blob can be accessed via the public internet (via a http link). After the specified time, the blob won't be accessible anymore via this link. This is a trade-off that you have to make yourself.

Interface method for generating temporary link (from IBlobStorageFacade.cs):
```
    public StorageItemInfo GetRawStorageItem(string path);
```

# Helper functions
This class library also contains some "helper classes". 
- FileHelper
- StorageHelper

These helper classes can come in handy when using the IBlobStorageFacade interface. The following methods are provided by these helpers:
```
// FileHelper
public static string GetContentType(this string fileName);
public static string GetFileExtension(this string fileName);

// StoragePathHelper
public static string GetContainerName(this string path);
public static string PathWithoutContainer(this string path);
public static string GetFileOfPath(this string path);
public static bool IsValidPath(this string path);
public static string FormatStoragePath(string containerName, string? directory, string imageName);
```

For a detailed explanation, see the code/implementation in the Github repo.

# All interface methods
This chapter contains a copy of the class that contains all the methods for the interface IBlobStorageFacade. Comments about this interface are present in code (see Github repo):
```
        public Task<bool> DeleteStorageItemAsync(string path);
        public bool DeleteStorageItem(string path);
        
        public Uri GetTempBlobLink(string path, int minutes = 1);
        
        public Task<Uri> UploadContentAsync(byte[] bytes, string path);
        public Task<Uri> UploadContentAsync(IFormFile file, string path);
        public Task<Uri> UploadContentAsync(Stream stream, string path);
        public Uri UploadContent(byte[] bytes, string path);
        public Uri UploadContent(IFormFile file, string path);
        public Uri UploadContent(Stream stream, string path);

        public StorageItemInfo GetRawStorageItem(string path);
        public Task<StorageItemInfo> GetRawStorageItemAsync(string path);
```

Below are the functions from the interface with explanation
```
    /// <summary>
    /// Interface for performing actions regarding the Azure Blob Storage. 
    /// Goal: Simplify the use of Azure Blob Storage - Write less do more
    /// 
    /// This interface uses the "path" keyword a lot. The path is the whole path to the file. The path includes the container name.
    /// Example {container-name}/img/profile-pictures/private-accounts/myitem.jpeg
    /// 
    /// </summary>
    public interface IBlobStorageFacade
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
        /// Get a temporary url that is accessible for an x amount of minutes (standard 1 minute).
        /// 
        /// This way private blobs can be made accessible via a url without a lot of internet traffic.
        /// The chances of someone "guessing" this url are extremely little, because the link consists of a lot of characters.
        /// 
        /// Private blobs can be retrieved "raw" too, but that way the traffic will increase significantly.
        /// </summary>
        public Uri GetTempBlobLink(string path, int minutes = 1);

        /// <summary>
        /// Upload content to the blob storage. 
        /// Content can be uploaded as a byte array, IFormFile or Stream.
        /// </summary>
        public Task<Uri> UploadContentAsByteArrayAsync(byte[] bytes, string path);
        public Task<Uri> UploadContentAsIFormFileAsync(IFormFile file, string path);
        public Task<Uri> UploadContentAsStreamAsync(Stream stream, string path);

        public Uri UploadContentAsByteArray(byte[] bytes, string path);
        public Uri UploadContentAsIFormFile(IFormFile file, string path);
        public Uri UploadContentAsStream(Stream stream, string path);

        /// <summary>
        /// Retrieve a storage item from the blob storage.
        /// Item will be retrieved as BinaryData. BinaryData can (for example) be converted to a Base64 string.
        /// The StorageItemInfo contains the Content Type of the file, so that the caller knows what kind of file is retrieved as BinaryData.
        /// </summary>
        public StorageItemInfo GetRawStorageItem(string path);
        public Task<StorageItemInfo> GetRawStorageItemAsync(string path);
```

# Contribute
Feel free to contribute! Suggestions are always welcome (e.g. different class types that can be uploaded to the blob storage).

## License

MIT
**Free Software, Hell Yeah!**
