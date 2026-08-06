/*using Ticketing.Application.DTOs;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Application.Interfaces.Services;
using Ticketing.Domain.Entities;
using Ticketing.Domain.Enums;

namespace Ticketing.Application.Features.Ticket;

public class CreateTicket
{
    private readonly ITicketRepository _ticketRepository;
    private readonly ICurrentUserService _currentUserService;

    public CreateTicket(ITicketRepository ticketRepository, ICurrentUserService currentUserService)
    {
        _ticketRepository = ticketRepository;
        _currentUserService = currentUserService;
    }

    public async Task<TicketResponseDto> HandleAsync(CreateTicketDto dto, Guid createdByUserId)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User must be authenticated to create a ticket.");
        }

        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description,
            DepartmentId = dto.DepartmentId,
            CategoryId = dto.CategoryId,
            Priority = dto.Priority,
            Status = TicketStatus.Open,
            CreatedById = _currentUserService.UserId.Value,
            CreatedAt = DateTime.UtcNow
        };

        await _ticketRepository.AddAsync(ticket);
        await _ticketRepository.SaveChangesAsync();

        return new TicketResponseDto
        {
            Id = ticket.Id,
            Title = ticket.Title,
            Description = ticket.Description,
            Status = ticket.Status.ToString(),
            Priority = ticket.Priority.ToString(),
            DepartmentId = ticket.DepartmentId,
            CategoryId = ticket.CategoryId,
            CreatedAt = ticket.CreatedAt
        };
    }
} */