using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ticketing.API.Common.Models;
using Ticketing.Application.DTOs.Notifications;
using Ticketing.Application.Features.Notifications.Commands.MarkAsRead;
using Ticketing.Application.Features.Notifications.Queries.GetNotifications;
using Ticketing.Application.Interfaces.Services;

namespace Ticketing.API.Controllers;

public class NotificationsController : BaseApiController
{
    private readonly ICurrentUserService _currentUserService;

    public NotificationsController(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    [HttpGet]
    [Authorize(Policy = "StaffOrAbove")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<NotificationDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNotifications(CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();
        var result = await Mediator.Send(new GetNotificationsQuery(userId), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<NotificationDto>>.Success(result, "Notifications retrieved successfully."));
    }

    [HttpPut("{id:guid}/read")]
    [Authorize(Policy = "StaffOrAbove")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();
        await Mediator.Send(new MarkNotificationAsReadCommand(id, userId), cancellationToken);
        return Ok(ApiResponse<object>.Success(new { Id = id }, "Notification marked as read."));
    }
}