using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Traninig_Managment_system.BLL.Helper
{
    public class FileService : IFileService
    {

        private readonly AzureBlobStorageOptions _options;
        private readonly BlobContainerClient _publicContainerClient;

        private readonly string[] _allowedExtensions =
        {
            ".jpg", ".jpeg", ".png", ".webp",
            ".pdf", ".docx", ".pptx",
            ".mp4", ".mov", ".webm"
        };

        private const long MaxFileSizeInBytes = 200 * 1024 * 1024; // 200 MB

        public FileService(IOptions<AzureBlobStorageOptions> options)
        {
            _options = options.Value;

            if (string.IsNullOrWhiteSpace(_options.ConnectionString))
                throw new Exception("Azure Blob Storage connection string is missing.");

            if (string.IsNullOrWhiteSpace(_options.PublicContainerName))
                throw new Exception("Azure Blob Storage public container name is missing.");

            _publicContainerClient = new BlobContainerClient(
                _options.ConnectionString,
                _options.PublicContainerName
            );
        }

        public async Task<string?> UploadFileAsync(IFormFile? file, string folderName)
        {
            if (file == null || file.Length == 0)
                return null;

            if (file.Length > MaxFileSizeInBytes)
                throw new Exception("File size is too large.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (string.IsNullOrEmpty(ext))
                throw new Exception("Invalid file.");

            if (!_allowedExtensions.Contains(ext))
                throw new Exception("File type is not allowed.");

            // تأكد إن الـ container موجود
            await _publicContainerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            // اسم فريد للملف
            string uniqueFileName = $"{Guid.NewGuid()}{ext}";

            // folderName هنا زي course-logos أو instructors أو categories
            string cleanFolderName = folderName.Trim('/').Replace("\\", "/");

            // شكل التخزين على Azure
            string blobName = $"{cleanFolderName}/{DateTime.UtcNow:yyyy/MM}/{uniqueFileName}";

            BlobClient blobClient = _publicContainerClient.GetBlobClient(blobName);

            await using var stream = file.OpenReadStream();

            await blobClient.UploadAsync(
                stream,
                new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = file.ContentType
                    }
                }
            );

            // ده الرابط اللي هيتخزن في الداتابيز ويتعرض في الـ View
            return blobClient.Uri.ToString();
        }

        public async Task<string?> UpdateFileAsync(
            IFormFile? newFile,
            string? oldFilePath,
            string folderName)
        {
            if (newFile == null || newFile.Length == 0)
            {
                return oldFilePath;
            }

            string? newPath = await UploadFileAsync(newFile, folderName);

            if (!string.IsNullOrEmpty(newPath))
            {
                DeleteFile(oldFilePath);
            }

            return newPath;
        }

        public void DeleteFile(string? filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            string? blobName = ExtractBlobName(filePath);

            if (string.IsNullOrWhiteSpace(blobName))
                return;

            BlobClient blobClient = _publicContainerClient.GetBlobClient(blobName);

            blobClient.DeleteIfExists();
        }

        private string? ExtractBlobName(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return null;

            // لو المخزن في الداتابيز URL كامل
            if (Uri.TryCreate(filePath, UriKind.Absolute, out var uri))
            {
                // مثال:
                // /public-assets/course-logos/2026/05/file.jpg
                var path = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/'));

                var containerPrefix = $"{_options.PublicContainerName}/";

                if (path.StartsWith(containerPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    return path.Substring(containerPrefix.Length);
                }

                return path;
            }

            // لو المخزن في الداتابيز blob name بس
            return filePath.TrimStart('/').Replace("\\", "/");
        }

























        //    private readonly IWebHostEnvironment _webHostEnvironment;

        //    public FileService(IWebHostEnvironment webHostEnvironment)
        //    {
        //       _webHostEnvironment = webHostEnvironment;
        //    }

        //    public async Task<string?> UploadFileAsync(IFormFile? file, string folderName) 
        //    {
        //        if (file == null || file.Length == 0) return null;

        //        // 1. Path Preparation

        //        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, folderName);
        //        if (!Directory.Exists(uploadsFolder)) //تأكد أن المجلد موجود، إذا لم يكن موجودًا قم بإنشائه
        //            Directory.CreateDirectory(uploadsFolder);

        //        // 2. Unique file name

        //        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();//بجيب امتداد الملف
        //        if (string.IsNullOrEmpty(ext))
        //            throw new Exception("Invalid file");
        //        string uniqueFileName = $"{Guid.NewGuid()}{ext}";//بعمل اسم فريد للملف عشان ما يحصل تعارض في الأسماء

        //        // 3. إنشاء المسار الكامل للملف قبل حفظه
        //        string fullPath = Path.Combine(uploadsFolder, uniqueFileName);

        //        // احفظ المسار 
        //        using var fileStream = new FileStream(fullPath, FileMode.Create);
        //        await file.CopyToAsync(fileStream);

        //        // Return relative path 
        //        return $"/{folderName}/{uniqueFileName}";
        //    }
        //    public async Task<string?> UpdateFileAsync(IFormFile? newFile, string? oldFilePath, string folderName)
        //    {

        //        if (newFile == null || newFile.Length == 0)
        //        {
        //            return oldFilePath;
        //        }

        //        string? newPath = await UploadFileAsync(newFile, folderName);

        //        if (!string.IsNullOrEmpty(newPath))
        //        {
        //            DeleteFile(oldFilePath);
        //        }
        //        return newPath;
        //    }
        //    public void DeleteFile(string? filePath)
        //    {

        //        if (string.IsNullOrEmpty(filePath)) return ;

        //        var normalizedPath = filePath.TrimStart('/');

        //        string fullPath = Path.Combine(_webHostEnvironment.WebRootPath, normalizedPath);

        //        if (File.Exists(fullPath))
        //        {
        //            File.Delete(fullPath);
        //        }
        //    }
        //}
    }
}
