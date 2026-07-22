using HRestaurant.Enum;

namespace HRestaurant.Extentions
{
    public static class FileExtention
    {
        public static bool CheckFileSize(this HRestaurant.DTOS.Common.FileUploadDTO file, FileSize fileSize, long size)
        {
            switch (fileSize)
            {
                case FileSize.Kb:
                    return file.Length <= size * 1024;
                case FileSize.Mb:
                    return file.Length <= size * 1024 * 1024;
                case FileSize.Gb:
                    return file.Length <= size * 1024 * 1024 * 1024;
            }
            return false;

        }


        public static void DeleteFile(this string filename, params string[] roots)
        {
            if (string.IsNullOrEmpty(filename)) return;

            string path = string.Empty;

            for (int i = 0; i < roots.Length; i++)
            {
                path = Path.Combine(path, roots[i]);
            }

            path = Path.Combine(path, filename);

            if (File.Exists(path)) File.Delete(path);

        }


        public static async Task<string> CreateFileAsync(this HRestaurant.DTOS.Common.FileUploadDTO file, params string[] roots)
        {
            string fileName = string.Concat(Guid.NewGuid().ToString(), file.FileName);

            string path = string.Empty;

            for (int i = 0; i < roots.Length; i++)
            {
                path = Path.Combine(path, roots[i]);
            }

            path = Path.Combine(path, fileName);

            using (FileStream fileStream = new(path, FileMode.Create))
            {
                await file.Content.CopyToAsync(fileStream);
            }

            return fileName;
        }



        public static bool CheckFileType(this HRestaurant.DTOS.Common.FileUploadDTO file, string type)
        {
            if (file.ContentType.Contains(type))
            {
                return true;
            }
            return false;
        }
    }
}
