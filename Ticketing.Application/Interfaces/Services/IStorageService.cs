namespace Ticketing.Application.Interfaces.Services;

public record FileUploadResult(string Url, string FileName, string OriginalFileName, string ContentType, long FileSizeBytes);

public interface IStorageService
{
    Task<FileUploadResult> UploadAsync(Stream fileStream, string fileName, string contentType, long fileSizeBytes, CancellationToken cancellationToken = default);
}