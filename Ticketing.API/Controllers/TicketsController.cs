using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ticketing.API.Common.Models;
using Ticketing.Application.DTOs;
using Ticketing.Application.DTOs.Messages;
using Ticketing.Application.Features.Messages.Commands.SendMessage;
using Ticketing.Application.Features.Messages.Queries.GetMessages;
using Ticketing.Application.Features.Tickets.Commands.AssignTicket;
using Ticketing.Application.Features.Tickets.Commands.CreateTicket;
using Ticketing.Application.Features.Tickets.Commands.DeleteTicket;
using Ticketing.Application.Features.Tickets.Commands.EscalateTicket;
using Ticketing.Application.Features.Tickets.Commands.UpdateTicket;
using Ticketing.Application.Features.Tickets.Queries.GetAssignedTickets;
using Ticketing.Application.Features.Tickets.Queries.GetMyTickets;
using Ticketing.Application.Features.Tickets.Queries.GetBreachedTickets;
using Ticketing.Application.Features.Tickets.Queries.GetTicketAnalytics;
using Ticketing.Application.Features.Tickets.Queries.GetTickets;
using Ticketing.Application.Features.Tickets.Queries.GetTicketById;
using Ticketing.Application.Features.Tickets.Queries.GetAvailableTickets;
using Ticketing.Application.Features.Tickets.Commands.ReassignTicket;
using Ticketing.Application.Features.Tickets.Commands.CloseTicket;
using Ticketing.Application.Interfaces.Services;
using Ticketing.Domain.Enums;

namespace Ticketing.API.Controllers;

public class TicketsController : BaseApiController
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IStorageService _storageService;

    public TicketsController(
        ICurrentUserService currentUserService,
        IStorageService storageService)
    {
        _currentUserService = currentUserService;
        _storageService = storageService;
    }

    [HttpPost]
    [Authorize(Policy = "StaffOrAbove")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateTicket(
        [FromForm] CreateTicketDto dto,
        IFormFile? attachment,
        CancellationToken cancellationToken)
    {
        string? attachmentUrl = null;
        if (attachment is not null && attachment.Length > 0)
        {
            var uploadResult = await _storageService.UploadAsync(
                attachment.OpenReadStream(),
                attachment.FileName,
                attachment.ContentType,
                attachment.Length,
                cancellationToken);
            attachmentUrl = uploadResult.Url;
        }

        var result = await Mediator.Send(
            new CreateTicketCommand(dto, attachmentUrl),
            cancellationToken);

        return Ok(ApiResponse<TicketResponseDto>.Success(result, "Ticket created successfully."));
    }

    [HttpGet]
    [Authorize(Policy = "AgentOrAbove")]
    public async Task<IActionResult> GetTickets(
        [FromQuery] Guid? departmentId,
        [FromQuery] Guid? categoryId,
        [FromQuery] TicketStatus? status,
        [FromQuery] TicketPriority? priority,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new GetTicketsQuery(
                departmentId,
                categoryId,
                status,
                priority,
                fromDate,
                toDate), cancellationToken);
        return Ok(ApiResponse<object>.Success(result, "Tickets retrieved successfully."));
    }

    [HttpGet("available")]
    [Authorize(Policy = "AgentOrAbove")]
    public async Task<IActionResult> GetAvailableTickets(Guid departmentId,CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new GetAvailableTicketsQuery(departmentId), cancellationToken);
        return Ok(ApiResponse<object>.Success(result, "Tickets retrieved successfully."));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "StaffOrAbove")]
    [ProducesResponseType(typeof(ApiResponse<TicketResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTicketById(Guid id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetTicketByIdQuery(id), cancellationToken);
        return Ok(ApiResponse<TicketResponseDto>.Success(result, "Ticket retrieved successfully."));
    }

    [HttpGet("my-tickets")]
    [Authorize(Policy = "StaffOrAbove")]
    public async Task<IActionResult> GetMyTickets(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetMyTicketsQuery(), cancellationToken);
        return Ok(ApiResponse<object>.Success(result, "My tickets retrieved successfully."));
    }

    [HttpGet("breached")]
[Authorize(Policy = "ManagerOrAdmin")]
[ProducesResponseType(typeof(ApiResponse<IReadOnlyList<TicketResponseDto>>), StatusCodes.Status200OK)]
public async Task<IActionResult> GetBreachedTickets(
    CancellationToken cancellationToken)
{
    var result = await Mediator.Send(
        new GetBreachedTicketsQuery(),
        cancellationToken);

    return StatusCode(
        StatusCodes.Status200OK,
        ApiResponse<IReadOnlyList<TicketResponseDto>>.Success(
            result,
            "Breached tickets retrieved successfully."));
}

    [HttpGet("assigned")]
    [Authorize(Policy = "AgentOrAbove")]
    public async Task<IActionResult> GetAssignedTickets(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetAssignedTicketsQuery(), cancellationToken);
        return Ok(ApiResponse<object>.Success(result, "Assigned tickets retrieved successfully."));
    }

    [HttpGet("analytics")]
    [Authorize(Policy = "ManagerOrAdmin")]
    public async Task<IActionResult> GetAnalytics(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetTicketAnalyticsQuery(fromDate, toDate), cancellationToken);
        return Ok(ApiResponse<object>.Success(result, "Analytics retrieved successfully."));
    }

    [HttpPut("{id:guid}/assign")]
    [Authorize(Policy = "AgentOrAbove")]
    public async Task<IActionResult> AssignTicket(
        Guid id,
        [FromBody] AssignTicketRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new AssignTicketCommand(id, request), cancellationToken);

        return Ok(ApiResponse<TicketResponseDto>.Success(
            result,
            "Ticket assigned successfully."));
    }

    [HttpPut("{id:guid}/reassign")]
    [Authorize(Policy = "AgentOrAbove")]
    public async Task<IActionResult> ReassignTicket(
    Guid id,
    [FromBody] AssignTicketRequestDto request,
    CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new ReassignTicketCommand(id, request), cancellationToken);

        return Ok(ApiResponse<TicketResponseDto>.Success(
            result,
            "Ticket reassigned successfully."));
    }

    [HttpPut("{id:guid}/close")]
    [Authorize(Policy = "AgentOrAbove")]
    [ProducesResponseType(typeof(ApiResponse<TicketResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CloseTicket(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new CloseTicketCommand(id), cancellationToken);

        return Ok(ApiResponse<TicketResponseDto>.Success(result, "Ticket resolved successfully."));
    }

    [HttpPut("{id:guid}/escalate")]
    [Authorize(Policy = "AgentOrAbove")]
    public async Task<IActionResult> Escalate(Guid id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new EscalateTicketCommand(id), cancellationToken);

        return Ok(ApiResponse<TicketResponseDto>.Success(
            result,
            "Ticket escalated and queued for reassignment."));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "StaffOrAbove")]
    [ProducesResponseType(typeof(ApiResponse<TicketResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateTicket(
        Guid id,
        [FromBody] UpdateTicketDto dto,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new UpdateTicketCommand(id, dto),
            cancellationToken);

        return Ok(ApiResponse<TicketResponseDto>.Success(
            result,
            "Ticket updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "StaffOrAbove")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteTicket(Guid id, CancellationToken cancellationToken)
    {
        await Mediator.Send(new DeleteTicketCommand(id), cancellationToken);

        return Ok(ApiResponse<object>.Success(new { id }, "Ticket deleted successfully."));
    }

    [HttpGet("{id:guid}/messages")]
    [Authorize(Policy = "StaffOrAbove")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<TicketMessageDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMessages(Guid id, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();
        var result = await Mediator.Send(new GetTicketMessagesQuery(id, userId), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<TicketMessageDto>>.Success(result, "Messages retrieved successfully."));
    }

    [HttpPost("{id:guid}/messages")]
    [Authorize(Policy = "StaffOrAbove")]
    [ProducesResponseType(typeof(ApiResponse<TicketMessageDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendMessage(Guid id, [FromForm] SendTicketMessageRequestDto request, IFormFile? attachment, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

        string? attachmentUrl = null;
        if (attachment is not null && attachment.Length > 0)
        {
            var uploadResult = await _storageService.UploadAsync(
                attachment.OpenReadStream(), attachment.FileName, attachment.ContentType, attachment.Length, cancellationToken);
            attachmentUrl = uploadResult.Url;
        }

        var command = new SendTicketMessageCommand(id, request.Message, request.IsInternal, attachmentUrl, userId);
        var result = await Mediator.Send(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<TicketMessageDto>.Success(result, "Message sent successfully."));
    }
}
