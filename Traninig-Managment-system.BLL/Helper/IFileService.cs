using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Traninig_Managment_system.BLL.Helper
{
    public interface IFileService
    {
        Task<string?> UploadFileAsync(IFormFile? file, string folderName, string[] allowedExtensions, long maxBytes);
        void DeleteFile(string? filePath);
        Task<string?> UpdateFileAsync(IFormFile? newFile, string? oldFilePath, string folderName, string[] allowedExtensions, long maxBytes, bool deleteOldFile = true);    }
}
