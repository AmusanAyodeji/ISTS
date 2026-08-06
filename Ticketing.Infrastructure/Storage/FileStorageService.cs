using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ticketing.Application.Interfaces.Services;

namespace Ticketing.Infrastructure.Storage;

public class FileStorageService : IStorageService
{
    private readonly string _uploadPath;
    private readonly long _maxFileSizeBytes;
    private readonly HashSet<string> _allowedExtensions;
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(IConfiguration configuration, IHostEnvironment environment, ILogger<FileStorageService> logger)
    {
        var configuredPath = configuration["Storage:UploadPath"] ?? "uploads";
        _uploadPath = Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(environment.ContentRootPath, configuredPath);

        _maxFileSizeBytes = (configuration.GetValue<long?>("Storage:MaxFileSizeMB") ?? 10) * 1024 * 1024;

        var extensions = configuration.GetSection("Storage:AllowedExtensions").Get<string[]>()
            ?? [".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt", ".zip"];

        _allowedExtensions = new HashSet<string>(extensions, StringComparer.OrdinalIgnoreCase);
        _logger = logger;
    }

    public async Task<FileUploadResult> UploadAsync(Stream fileStream, string fileName, string contentType, long fileSizeBytes, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName);
        if (!_allowedExtensions.Contains(extension))
            throw new InvalidOperationException($"File extension '{extension}' is not allowed.");

        if (fileSizeBytes > _maxFileSizeBytes)
            throw new InvalidOperationException($"File size exceeds the maximum allowed size of {_maxFileSizeBytes / (1024 * 1024)} MB.");

        var dateFolder = DateTime.UtcNow.ToString("yyyyMMdd");
        var directory = Path.Combine(_uploadPath, dateFolder);
        Directory.CreateDirectory(directory);

        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(directory, uniqueFileName);

        using var outputStream = new FileStream(filePath, FileMode.Create);
        await fileStream.CopyToAsync(outputStream, cancellationToken);

        var relativePath = $"/{dateFolder}/{uniqueFileName}";
        _logger.LogInformation("File uploaded: {Path}", relativePath);

        return new FileUploadResult(relativePath, uniqueFileName, fileName, contentType, fileSizeBytes);
    }
}