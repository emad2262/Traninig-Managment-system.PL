
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Traninig_Managment_system.BLL.Helper
{
    public class FileService : IFileService
    {


        private readonly IWebHostEnvironment _webHostEnvironment;

        public FileService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<string?> UploadFileAsync(IFormFile? file, string folderName)
        {
            if (file == null || file.Length == 0) return null;

            // 1. Path Preparation

            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, folderName);
            if (!Directory.Exists(uploadsFolder)) //تأكد أن المجلد موجود، إذا لم يكن موجودًا قم بإنشائه
                Directory.CreateDirectory(uploadsFolder);

            // 2. Unique file name

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();//بجيب امتداد الملف
            if (string.IsNullOrEmpty(ext))
                throw new Exception("Invalid file");
            string uniqueFileName = $"{Guid.NewGuid()}{ext}";//بعمل اسم فريد للملف عشان ما يحصل تعارض في الأسماء

            // 3. إنشاء المسار الكامل للملف قبل حفظه
            string fullPath = Path.Combine(uploadsFolder, uniqueFileName);

            // احفظ المسار 
            using var fileStream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(fileStream);

            // Return relative path 
            return $"/{folderName}/{uniqueFileName}";
        }
        public async Task<string?> UpdateFileAsync(IFormFile? newFile, string? oldFilePath, string folderName)
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

            if (string.IsNullOrEmpty(filePath)) return;

            var normalizedPath = filePath.TrimStart('/');

            string fullPath = Path.Combine(_webHostEnvironment.WebRootPath, normalizedPath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }
}

