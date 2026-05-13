using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace Sandoohouse.Service;

public class CloudinaryService
{
    private readonly Cloudinary? _cloudinary;
    private readonly bool _isConfigured;

    public CloudinaryService(IConfiguration configuration)
    {
        var cloudName = configuration["Cloudinary:CloudName"];
        var apiKey    = configuration["Cloudinary:ApiKey"];
        var apiSecret = configuration["Cloudinary:ApiSecret"];

        // Only initialize if all 3 credentials are present and not placeholders
        _isConfigured = !string.IsNullOrWhiteSpace(cloudName)
                     && !string.IsNullOrWhiteSpace(apiKey)
                     && !string.IsNullOrWhiteSpace(apiSecret)
                     && cloudName != "YOUR_CLOUD_NAME";

        if (_isConfigured)
        {
            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true;
        }
        else
        {
            Console.WriteLine("[CloudinaryService] Cloudinary is not configured. Image uploads will be skipped.");
        }
    }

    /// <summary>
    /// Uploads an image to Cloudinary. Returns null if Cloudinary is not configured.
    /// </summary>
    public async Task<string?> UploadImageAsync(IFormFile file, string folder = "uploads")
    {
        if (!_isConfigured || _cloudinary == null)
        {
            Console.WriteLine("[CloudinaryService] Skipping upload — Cloudinary not configured.");
            return null;
        }

        if (file == null || file.Length == 0)
            return null;

        await using var stream = file.OpenReadStream();

        var uploadParams = new ImageUploadParams
        {
            File           = new FileDescription(file.FileName, stream),
            Folder         = folder,
            UseFilename    = false,
            UniqueFilename = true,
        };

        var result = await _cloudinary.UploadAsync(uploadParams);

        if (result.Error != null)
            throw new Exception($"Cloudinary upload failed: {result.Error.Message}");

        return result.SecureUrl.ToString();
    }

    /// <summary>
    /// Deletes an image from Cloudinary. Safely skips if not configured.
    /// </summary>
    public async Task DeleteImageAsync(string? imageUrl)
    {
        if (!_isConfigured || _cloudinary == null || string.IsNullOrWhiteSpace(imageUrl))
            return;

        try
        {
            var uri      = new Uri(imageUrl);
            var segments = uri.AbsolutePath.Split('/');

            int uploadIdx = Array.IndexOf(segments, "upload");
            if (uploadIdx < 0) return;

            var afterUpload = segments.Skip(uploadIdx + 1).ToArray();
            if (afterUpload.Length > 0 && afterUpload[0].StartsWith("v") && long.TryParse(afterUpload[0][1..], out _))
                afterUpload = afterUpload.Skip(1).ToArray();

            var publicIdWithExt = string.Join("/", afterUpload);
            var publicId        = Path.ChangeExtension(publicIdWithExt, null).TrimEnd('.');

            await _cloudinary.DestroyAsync(new DeletionParams(publicId));
        }
        catch
        {
            // Swallow deletion errors
        }
    }
}