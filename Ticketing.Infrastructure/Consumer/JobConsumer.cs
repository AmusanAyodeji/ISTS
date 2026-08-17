using MassTransit;
using Ticketing.Domain.Enums;
using Ticketing.Application.DTOs.Users;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Application.Interfaces.Services;

namespace Ticketing.Infrastructure.Consumer;

public class JobConsumer : IConsumer<BulkJobDTO>
{
    private readonly IJobRepository _jobRepository;
    private readonly IValidateRecord _validateRecord;
    private readonly IJobErrorRepository _jobErrorRepository;
    private readonly INotificationHubService _notificationHubService;

    public JobConsumer(IJobRepository jobRepository, IValidateRecord validateRecord, IJobErrorRepository jobErrorRepository, INotificationHubService notificationHubService)
    {
        _jobRepository = jobRepository;
        _validateRecord = validateRecord;
        _jobErrorRepository = jobErrorRepository;
        _notificationHubService = notificationHubService;
    }
    public async Task Consume(ConsumeContext<BulkJobDTO> context)
    {
        Console.WriteLine("got to consumer");
        var message = context.Message;
        Console.WriteLine(message.JobId);
        Console.WriteLine(JobStatus.Processing);
        await _jobRepository.UpdateStatusByJobIdAsync(message.JobId, JobStatus.Processing, context.CancellationToken);
        await _jobRepository.SaveChangesAsync(context.CancellationToken);
        await _validateRecord.PerformValidationAndSave(message.Userdata, message.JobId, message.DepartmentId, message.FileName, context.CancellationToken);
        await _jobErrorRepository.SaveChangesAsync(context.CancellationToken);
        await _jobRepository.UpdateStatusByJobIdAsync(message.JobId, JobStatus.Completed, context.CancellationToken);

        await _jobRepository.SaveChangesAsync(context.CancellationToken);
        await _jobErrorRepository.SaveChangesAsync(context.CancellationToken);
        await _notificationHubService.LoadResultsandErrors(message.UserId, message.JobId);
    }
}