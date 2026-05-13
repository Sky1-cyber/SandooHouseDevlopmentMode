namespace Sandoohouse.Helpers;

public class FileUploadHelper
{
    public static async Task<string?> UploadImage(IFormFile? file, string folderName)
    {
        if (file == null || file.Length == 0)
            return null;

        string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folderName);

        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

        string filePath = Path.Combine(uploadsFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return fileName;
    }
}