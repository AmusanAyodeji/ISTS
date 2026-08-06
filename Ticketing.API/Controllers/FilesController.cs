using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ticketing.API.Common.Models;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Application.Interfaces.Services;
using Ticketing.Domain.Entities;

namespace Ticketing.API.Controllers;

public class FilesController : BaseApiController
{
    private readonly IStorageService _storageService;
    private readonly IFileAttachmentRepository _fileAttachmentRepository;
    private readonly ICurrentUserService _currentUserService;

    public FilesController(
        IStorageService storageService,
        IFileAttachmentRepository fileAttachmentRepository,
        ICurrentUserService currentUserService)
    {
        _storageService = storageService;
        _fileAttachmentRepository = fileAttachmentRepository;
        _currentUserService = currentUserService;
    }

    [HttpPost("upload")]
    [Authorize(Policy = "StaffOrAbove")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken cancellationToken)
    {
        if (file.Length == 0)
            return BadRequest(ApiResponse<object>.Failure(["File is empty."], "Upload failed."));

        var result = await _storageService.UploadAsync(
            file.OpenReadStream(), file.FileName, file.ContentType, file.Length, cancellationToken);

        var attachment = new FileAttachment
        {
            FileName = result.FileName,
            OriginalFileName = result.OriginalFileName,
            FilePath = result.Url,
            ContentType = result.ContentType,
            FileSizeBytes = result.FileSizeBytes,
            UploadedByUserId = _currentUserService.UserId
        };

        await _fileAttachmentRepository.AddAsync(attachment, cancellationToken);
        await _fileAttachmentRepository.SaveChangesAsync(cancellationToken);

        return Ok(ApiResponse<object>.Success(new
        {
            attachment.Id,
            result.Url,
            result.OriginalFileName,
            result.ContentType,
            result.FileSizeBytes
        }, "File uploaded successfully."));
    }
}