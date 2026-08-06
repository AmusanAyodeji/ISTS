using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ticketing.Application.Interfaces.Services;
using Ticketing.Infrastructure.Options;

namespace Ticketing.Infrastructure.Storage;

public class CloudinaryStorageService : IStorageService
{
    private readonly Cloudinary _cloudinary;
    private readonly long _maxFileSizeBytes;
    private readonly HashSet<string> _allowedExtensions;
    private readonly ILogger<CloudinaryStorageService> _logger;

    public CloudinaryStorageService(
        IOptions<CloudinarySettings> options,
        IConfiguration configuration,
        ILogger<CloudinaryStorageService> logger)
    {
        var settings = options.Value;

        if (string.IsNullOrWhiteSpace(settings.CloudName))
            throw new InvalidOperationException("Cloudinary:CloudName is not configured.");
        if (string.IsNullOrWhiteSpace(settings.ApiKey))
            throw new InvalidOperationException("Cloudinary:ApiKey is not configured.");
        if (string.IsNullOrWhiteSpace(settings.ApiSecret))
            throw new InvalidOperationException("Cloudinary:ApiSecret is not configured.");

        _cloudinary = new Cloudinary(new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret));

        _maxFileSizeBytes = (configuration.GetValue<long?>("Storage:MaxFileSizeMB") ?? 10) * 1024 * 1024;

        var extensions = configuration.GetSection("Storage:AllowedExtensions").Get<string[]>()
            ?? [".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt", ".zip"];

        _allowedExtensions = new HashSet<string>(extensions, StringComparer.OrdinalIgnoreCase);
        _logger = logger;
    }

    public async Task<FileUploadResult> UploadAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        long fileSizeBytes,
        CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(extension) || !_allowedExtensions.Contains(extension))
            throw new InvalidOperationException($"File extension '{extension}' is not allowed.");

        if (fileSizeBytes > _maxFileSizeBytes)
            throw new InvalidOperationException($"File size exceeds the maximum allowed size of {_maxFileSizeBytes / (1024 * 1024)} MB.");

        var dateFolder = DateTime.UtcNow.ToString("yyyyMMdd");
        var basePublicId = $"ists/{dateFolder}/{Guid.NewGuid()}";

        UploadResult uploadResult;
        var resourceType = GetResourceType(extension);

        if (resourceType == "image")
        {
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, fileStream),
                PublicId = basePublicId,
                Overwrite = false
            };

            uploadResult = await _cloudinary.UploadAsync(uploadParams, cancellationToken);
        }
        else if (resourceType == "video")
        {
            var uploadParams = new VideoUploadParams
            {
                File = new FileDescription(fileName, fileStream),
                PublicId = basePublicId,
                Overwrite = false
            };

            uploadResult = await _cloudinary.UploadAsync(uploadParams, cancellationToken);
        }
        else
        {
            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(fileName, fileStream),
                PublicId = $"{basePublicId}{extension}",
                Overwrite = false
            };

            uploadResult = await _cloudinary.UploadAsync(uploadParams, "raw", cancellationToken);
        }

        if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
        {
            var error = uploadResult.Error?.Message ?? "Cloudinary upload failed.";
            throw new InvalidOperationException($"Cloudinary upload failed: {error}");
        }

        var secureUrl = uploadResult.SecureUrl?.AbsoluteUri
            ?? uploadResult.Url?.AbsoluteUri
            ?? throw new InvalidOperationException("Cloudinary did not return a file URL.");

        _logger.LogInformation(
            "File uploaded to Cloudinary. PublicId={PublicId}, Url={Url}, ResourceType={ResourceType}",
            uploadResult.PublicId,
            secureUrl,
            resourceType);

        return new FileUploadResult(secureUrl, uploadResult.PublicId, fileName, contentType, fileSizeBytes);
    }

    private static string GetResourceType(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" or ".bmp" or ".svg" or ".ico" => "image",
            ".mp4" or ".mov" or ".avi" or ".mkv" or ".webm" => "video",
            _ => "raw"
        };
    }
}
