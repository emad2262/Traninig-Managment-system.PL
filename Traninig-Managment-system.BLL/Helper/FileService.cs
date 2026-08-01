// =====================================================================
// BLL/Helper/FileRules.cs
// قوائم مسموحة وأحجام قصوى — مكان واحد بدل ما تتكرر في كل كنترولر
// =====================================================================

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

//namespace Traninig_Managment_system.BLL.Helper
//{
//    public static class FileRules
//    {
//        public static readonly string[] Images = { ".jpg", ".jpeg", ".png", ".webp" };
//        public static readonly string[] Videos = { ".mp4", ".webm", ".mov" };
//        public static readonly string[] Documents = { ".pdf" };

//        public const long MaxImageBytes = 2L * 1024 * 1024;        // 2 MB
//        public const long MaxDocumentBytes = 20L * 1024 * 1024;    // 20 MB
//        public const long MaxVideoBytes = 500L * 1024 * 1024;      // 500 MB
//    }
//}

namespace Traninig_Managment_system.BLL.Helper
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public FileService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<string?> UploadFileAsync(IFormFile? file,string folderName,string[] allowedExtensions,long maxBytes)
        {
            if (file is null || file.Length == 0)
                return null;

            // 1. الحجم — قبل أي حاجة تانية عشان مندخلش في I/O من غير داعي
            if (file.Length > maxBytes)
                throw new InvalidOperationException(
                    $"The file is larger than the {maxBytes / (1024 * 1024)} MB limit.");

            // 2. الامتداد — allowlist، مش blocklist.
            //    الـ FileName جاي من العميل فمش موثوق: بناخد الامتداد منه بس
            //    وبنتحقق منه، ومبنستخدمش الاسم نفسه خالص.
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (string.IsNullOrEmpty(ext) || !allowedExtensions.Contains(ext))
                throw new InvalidOperationException(
                    $"Only {string.Join(", ", allowedExtensions)} files are allowed.");

            // 3. الفولدر — متحقَّق منه إنه جوه wwwroot
            var uploadsFolder = ResolveFolderInsideWebRoot(folderName);
            Directory.CreateDirectory(uploadsFolder);   // بيتجاهل لو موجود

            // 4. اسم فريد — GUID، فمفيش تعارض ومفيش أي حرف من اسم العميل
            var uniqueFileName = $"{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(uploadsFolder, uniqueFileName);

            // 5. الحفظ — بـ useAsync ومساحة بافر أكبر عشان الملفات الكبيرة
            try
            {
                await using var fileStream = new FileStream(
                    fullPath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None,
                    bufferSize: 64 * 1024,
                    useAsync: true);

                await file.CopyToAsync(fileStream);
            }
            catch
            {
                // لو الرفع اتقطع في النص، منسيبش ملف نص نص على الديسك
                if (File.Exists(fullPath))
                    File.Delete(fullPath);

                throw;
            }

            // المسار النسبي دايمًا بـ forward slashes عشان يشتغل كـ URL
            return $"/{folderName.Replace('\\', '/').Trim('/')}/{uniqueFileName}";
        }

        public async Task<string?> UpdateFileAsync(IFormFile? newFile,string? oldFilePath,string folderName,string[] allowedExtensions,long maxBytes,bool deleteOldFile = true)
        {
            // مرفعش حاجة جديدة → القديم يفضل زي ما هو
            if (newFile is null || newFile.Length == 0)
                return oldFilePath;

            var newPath = await UploadFileAsync(newFile, folderName, allowedExtensions, maxBytes);

            // الترتيب مهم: بنمسح القديم بعد ما نتأكد إن الجديد اتحفظ
            if (deleteOldFile && !string.IsNullOrEmpty(newPath))
                DeleteFile(oldFilePath);

            return newPath;
        }

        public void DeleteFile(string? filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            var webRoot = GetWebRoot();
            var relative = filePath.Replace('\\', '/').TrimStart('/');

            var fullPath = Path.GetFullPath(Path.Combine(webRoot, relative));

            // الحارس: لو المسار طلع بره wwwroot، مبنعملش حاجة.
            // ده اللي بيمنع "/../appsettings.json" من إنه يمسح ملفات النظام.
            if (!IsInside(webRoot, fullPath))
                return;

            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }

        // -----------------------------------------------------------------
        //  Helpers
        // -----------------------------------------------------------------

        private string GetWebRoot()
        {
            var root = _webHostEnvironment.WebRootPath;

            // بيرجع null لو مفيش فولدر wwwroot وقت الـ startup
            if (string.IsNullOrEmpty(root))
            {
                root = Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot");
                Directory.CreateDirectory(root);
            }

            return Path.GetFullPath(root);
        }

        private string ResolveFolderInsideWebRoot(string folderName)
        {
            if (string.IsNullOrWhiteSpace(folderName))
                throw new InvalidOperationException("The upload folder is required.");

            var webRoot = GetWebRoot();
            var full = Path.GetFullPath(Path.Combine(webRoot, folderName));

            if (!IsInside(webRoot, full))
                throw new InvalidOperationException("Invalid upload folder.");

            return full;
        }

        private static bool IsInside(string root, string candidate)
        {
            var normalizedRoot = root.TrimEnd(Path.DirectorySeparatorChar)
                                     + Path.DirectorySeparatorChar;

            return candidate.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase);
        }
    }
}