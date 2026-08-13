using System;
using System.Threading;
using AutoMapper;
using MediatR;
using Ticketing.Application.DTOs;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Application.Interfaces.Services;
using Ticketing.Domain.Entities;
using Ticketing.Domain.Enums;

namespace Ticketing.Application.Features.Tickets.Commands.CreateTicket;

public class CreateTicketCommandHandler : IRequestHandler<CreateTicketCommand, TicketResponseDto>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ISLARepository _slaRepository;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly INotificationHubService _notificationHubService;

    public CreateTicketCommandHandler(
        ITicketRepository ticketRepository,
        ICategoryRepository categoryRepository,
        ISLARepository slaRepository,
        IMapper mapper,
        ICurrentUserService currentUserService,
        INotificationHubService notificationHubService)
    {
        _ticketRepository = ticketRepository;
        _categoryRepository = categoryRepository;
        _slaRepository = slaRepository;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _notificationHubService = notificationHubService;
    }

    public async Task<TicketResponseDto> Handle(CreateTicketCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User must be authenticated to create a ticket.");
        }

        var isValidCategory = await _categoryRepository.CategoryBelongsToDepartmentAsync(
            request.Request.CategoryId,
            request.Request.DepartmentId,
            cancellationToken);

        if (!isValidCategory)
        {
            throw new InvalidOperationException(
                "The selected category does not belong to the selected department.");
        }

        var ticket = _mapper.Map<Ticket>(request.Request);
        ticket.Id = Guid.NewGuid();
        ticket.Status = TicketStatus.Open;
        ticket.CreatedById = _currentUserService.UserId.Value;
        ticket.CreatedAt = DateTime.UtcNow;
        ticket.AttachmentUrl = request.AttachmentUrl;

        var sla = await _slaRepository.GetSLAByPriority(request.Request.DepartmentId, request.Request.Priority, cancellationToken);
        var resolutionMinutes = sla?.ResolutionTimeMinutes ?? GetDefaultResolutionMinutes(request.Request.Priority);
        ticket.SlaDueAt = ticket.CreatedAt.AddMinutes(resolutionMinutes);

        await _ticketRepository.AddAsync(ticket, cancellationToken);
        await _ticketRepository.SaveChangesAsync(cancellationToken);

        var createdTicket = await _ticketRepository.GetByIdWithDetailsAsync(ticket.Id, cancellationToken);
        var returnTicket = _mapper.Map<TicketResponseDto>(createdTicket ?? ticket);
        await _notificationHubService.AddTicketToQueueAsync(returnTicket);
        return returnTicket;
    }

    private static int GetDefaultResolutionMinutes(TicketPriority priority)
    {
        return priority switch
        {
            TicketPriority.Low => 24 * 60,
            TicketPriority.Medium => 8 * 60,
            TicketPriority.High => 4 * 60,
            TicketPriority.Urgent => 60,
            _ => 8 * 60
        };
    }
}
