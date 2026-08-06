using AutoMapper;
using MediatR;
using Ticketing.Application.DTOs;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Application.Interfaces.Services;
using Ticketing.Domain.Entities;
using Ticketing.Domain.Enums;

namespace Ticketing.Application.Features.Tickets.Commands.UpdateTicket;

public class UpdateTicketCommandHandler : IRequestHandler<UpdateTicketCommand, TicketResponseDto>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ISLARepository _slaRepository;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public UpdateTicketCommandHandler(
        ITicketRepository ticketRepository,
        ICategoryRepository categoryRepository,
        ISLARepository slaRepository,
        IMapper mapper,
        ICurrentUserService currentUserService)
    {
        _ticketRepository = ticketRepository;
        _categoryRepository = categoryRepository;
        _slaRepository = slaRepository;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<TicketResponseDto> Handle(UpdateTicketCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User must be authenticated to update a ticket.");
        }

        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken)
            ?? throw new InvalidOperationException("Ticket not found.");

        if (ticket.CreatedById != _currentUserService.UserId.Value)
        {
            throw new UnauthorizedAccessException("You can only edit tickets you created.");
        }

        if (ticket.Status == TicketStatus.Resolved || ticket.Status == TicketStatus.Closed)
        {
            throw new InvalidOperationException("Resolved or closed tickets cannot be edited.");
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

        ticket.Title = request.Request.Title;
        ticket.Description = request.Request.Description;
        ticket.Priority = request.Request.Priority;
        ticket.DepartmentId = request.Request.DepartmentId;
        ticket.CategoryId = request.Request.CategoryId;

        // Recalculate SLA due date in case priority changed.
        var sla = await _slaRepository.GetSLAByPriority(
            request.Request.DepartmentId,
            request.Request.Priority,
            cancellationToken);
        var resolutionMinutes = sla?.ResolutionTimeMinutes ?? GetDefaultResolutionMinutes(request.Request.Priority);
        ticket.SlaDueAt = ticket.CreatedAt.AddMinutes(resolutionMinutes);

        _ticketRepository.Update(ticket);
        await _ticketRepository.SaveChangesAsync(cancellationToken);

        var updatedTicket = await _ticketRepository.GetByIdWithDetailsAsync(request.TicketId, cancellationToken);
        return _mapper.Map<TicketResponseDto>(updatedTicket ?? ticket);
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
