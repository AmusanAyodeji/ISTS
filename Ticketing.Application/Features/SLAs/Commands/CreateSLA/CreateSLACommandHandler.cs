using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ticketing.Application.DTOs.Notifications;
using Ticketing.Application.DTOs.SLA;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Application.Interfaces.Services;
using Ticketing.Domain.Entities;
using Ticketing.Domain.Enums;

namespace Ticketing.Application.Features.SLAs.Commands.CreateSLA;

public class CreateSLACommandHandler : IRequestHandler<CreateSLACommand, CreateSLAResponseDTO>
{
    private readonly ISLARepository _slaRepository;
    private readonly IMapper _mapper;
    private readonly INotificationRepository _notificationRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationHubService _notificationHubService;

    public CreateSLACommandHandler(
        ISLARepository slaRepository,
        IMapper mapper,
        INotificationRepository notificationRepository,
        IUserRepository userRepository,
        INotificationHubService notificationHubService)
    {
        _slaRepository = slaRepository;
        _mapper = mapper;
        _notificationRepository = notificationRepository;
        _userRepository = userRepository;
        _notificationHubService = notificationHubService;
    }

    public async Task<CreateSLAResponseDTO> Handle(CreateSLACommand request, CancellationToken cancellationToken)
    {
        var priorities = request.Request.Priorities;
        if (priorities is null || priorities.Count == 0)
        {
            throw new ArgumentException("At least one priority SLA must be provided.");
        }

        var slaEntities = priorities.Select(priority => new SLA
        {
            DepartmentId = request.Request.DepartmentId,
            Priority = priority.Priority,
            ResponseTimeMinutes = priority.ResponseTimeMinutes,
            ResolutionTimeMinutes = priority.ResolutionTimeMinutes
        }).ToList();

        var existingSlas = await _slaRepository.GetSLA(
            request.Request.DepartmentId,
            cancellationToken
        );

        foreach (var sla in slaEntities)
        {
            var priority = sla.Priority;
            if (priority == default)
            {
                throw new ArgumentException("Priority must be specified for each SLA entry.");
            }

#pragma warning disable CS8602
            var exists = existingSlas.Any(existing =>
                existing.DepartmentId == sla.DepartmentId &&
                existing.Priority == priority
            );
#pragma warning restore CS8602

            if (exists)
            {
                throw new InvalidOperationException($"SLA already exists for priority: {priority}");
            }
        }

        foreach (var sla in slaEntities)
        {
            await _slaRepository.AddAsync(sla, cancellationToken);
        }

        await _slaRepository.SaveChangesAsync(cancellationToken);

        await NotifyDepartmentManagersAsync(request.Request.DepartmentId, cancellationToken);

        return new CreateSLAResponseDTO
        {
            DepartmentId = request.Request.DepartmentId,
            SLAs = slaEntities.Select(sla => _mapper.Map<SLAResponseItemDTO>(sla)).ToList()
        };
    }

    private async Task NotifyDepartmentManagersAsync(Guid departmentId, CancellationToken cancellationToken)
    {
        var managers = await _userRepository.GetManagersByDepartmentAsync(departmentId, cancellationToken);
        if (managers.Count == 0)
        {
            return;
        }

        const string title = "SLA rules created";
        const string message = "New SLA rules have been configured for your department.";

        foreach (var manager in managers)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = manager.Id,
                Title = title,
                Message = message,
                Type = NotificationType.Info,
                TicketId = null
            };

            await _notificationRepository.AddAsync(notification, cancellationToken);

            var notificationDto = new NotificationDto
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                IsRead = notification.IsRead,
                ReadAt = notification.ReadAt,
                TicketId = notification.TicketId,
                CreatedAt = notification.CreatedAt
            };

            await _notificationHubService.NotifyUserAsync(manager.Id, notificationDto);
        }

        await _notificationRepository.SaveChangesAsync(cancellationToken);
    }
}
