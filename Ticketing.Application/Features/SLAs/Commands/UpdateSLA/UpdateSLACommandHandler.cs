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

namespace Ticketing.Application.Features.SLAs.Commands.UpdateSLA
{
    public class UpdateSLACommandHandler : IRequestHandler<UpdateSLACommand, UpdateSLAResponseDTO>
    {
        private readonly IMapper _mapper;
        private readonly ISLARepository _slaRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IUserRepository _userRepository;
        private readonly INotificationHubService _notificationHubService;

        public UpdateSLACommandHandler(
            IMapper mapper,
            ISLARepository slaRepository,
            INotificationRepository notificationRepository,
            IUserRepository userRepository,
            INotificationHubService notificationHubService)
        {
            _mapper = mapper;
            _slaRepository = slaRepository;
            _notificationRepository = notificationRepository;
            _userRepository = userRepository;
            _notificationHubService = notificationHubService;
        }

        public async Task<UpdateSLAResponseDTO> Handle(UpdateSLACommand request, CancellationToken cancellationToken)
        {
            var priorities = request.Request.Priorities;
            if (priorities is null || priorities.Count == 0)
            {
                throw new ArgumentException("At least one priority SLA must be provided.");
            }

            var slas = await _slaRepository.GetSLA(request.Request.DepartmentId, cancellationToken);
            if (slas.Count == 0)
            {
                throw new InvalidOperationException("SLAs doesn't exist");
            }

            foreach (var currentpriority in slas)
            {
#pragma warning disable CS8602
                var updated = priorities
                    .FirstOrDefault(priority => priority.Priority == currentpriority.Priority)
                    ?? throw new InvalidOperationException($"Updated SLA data for priority {currentpriority.Priority} was not provided.");

                currentpriority.ResponseTimeMinutes = updated.ResponseTimeMinutes;
                currentpriority.ResolutionTimeMinutes = updated.ResolutionTimeMinutes;
#pragma warning restore CS8602
                currentpriority.UpdatedAt = DateTime.UtcNow;
                _slaRepository.Update(currentpriority);
            }

            await _slaRepository.SaveChangesAsync(cancellationToken);

            await NotifyDepartmentManagersAsync(request.Request.DepartmentId, cancellationToken);

            return new UpdateSLAResponseDTO
            {
                DepartmentId = request.Request.DepartmentId,
                SLAs = slas.Select(sla => _mapper.Map<SLAResponseItemDTO>(sla)).ToList()
            };
        }

        private async Task NotifyDepartmentManagersAsync(Guid departmentId, CancellationToken cancellationToken)
        {
            var managers = await _userRepository.GetManagersByDepartmentAsync(departmentId, cancellationToken);
            if (managers.Count == 0)
            {
                return;
            }

            const string title = "SLA rules updated";
            const string message = "The SLA rules for your department have been updated.";

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
}
