namespace Traninig_Managment_system.Utality
{
    public static class Upload
    {
        public static class FileUploadHelper
        {
            public static string Upload(IFormFile file, string folder)
            {
                if (file == null || file.Length == 0)
                {
                    throw new Exception("Invalid file");

                }
                string fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folder, fileName);
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
                using var stream = new FileStream(fullPath, FileMode.Create);
                file.CopyTo(stream);

                return $"/{folder}/{fileName}";
            }
        }
    }
}
