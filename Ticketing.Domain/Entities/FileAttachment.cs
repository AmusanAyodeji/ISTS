using Ticketing.Domain.Common;

namespace Ticketing.Domain.Entities;

public class FileAttachment : BaseEntity
{
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public Guid? UploadedByUserId { get; set; }
    public User? UploadedByUser { get; set; }
}