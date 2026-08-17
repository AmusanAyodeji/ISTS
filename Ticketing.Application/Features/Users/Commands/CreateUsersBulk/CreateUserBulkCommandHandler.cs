using MassTransit;
using Ticketing.Domain.Enums;
using Ticketing.Domain.Entities;
using Ticketing.Application.DTOs.Users;
using System.Globalization;
using System.IO;
using MediatR;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Application.Interfaces.Services;

namespace Ticketing.Application.Features.Users.Commands.CreateUsersBulk;

public class CreateUserBulkCommandHandler : IRequestHandler<CreateUsersBulkCommand, BulkResponseDTO>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IJobRepository _jobRepository;
    public readonly ICurrentUserService _currentUserService;

    public CreateUserBulkCommandHandler(IPublishEndpoint publishEndpoint, IJobRepository jobRepository, ICurrentUserService currentUserService)
    {
        _publishEndpoint = publishEndpoint;
        _jobRepository = jobRepository;
        _currentUserService = currentUserService;
    }

    public async Task<BulkResponseDTO> Handle(CreateUsersBulkCommand request, CancellationToken cancellationToken)
    {
        var JobId = Guid.NewGuid();
        var response = new BulkResponseDTO { JobId = JobId, DepartmentId = request.DepartmentId, Status = JobStatus.Queued.ToString() };
        await _jobRepository.AddAsync(new Job { JobId = JobId, DepartmentId = request.DepartmentId, Status = JobStatus.Queued, FileName = request.FileName }, cancellationToken);
        await _jobRepository.SaveChangesAsync(cancellationToken);
        await _publishEndpoint.Publish(new BulkJobDTO
        {
            JobId = JobId,
            FileName = request.FileName,
            DepartmentId = request.DepartmentId,
            Userdata = request.userinfo,
            TotalRows = request.userinfo.Count(),
            UserId = _currentUserService.UserId.Value
        });
        return response;
    }
}