using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ticketing.API.Common.Models;
using Ticketing.Application.DTOs.Users;
using Ticketing.Application.Features.Users.Commands.CreateUser;
using Ticketing.Application.Features.Users.Commands.DeleteUser;
using Ticketing.Application.Features.Users.Commands.UpdateUser;
using Ticketing.Application.Features.Users.Queries.GetAgents;
using Ticketing.Application.Features.Users.Queries.GetCurrentUser;
using Ticketing.Application.Features.Users.Queries.GetUsers;

namespace Ticketing.API.Controllers;

public class UsersController : BaseApiController
{
    [HttpGet]
    [Authorize(Policy = "AgentOrAbove")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<UserDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetUsersQuery(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<UserDto>>.Success(result, "Users retrieved successfully."));
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<CurrentUserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetCurrentUserQuery(), cancellationToken);
        return Ok(ApiResponse<CurrentUserDto>.Success(result, "Current user retrieved successfully."));
    }

    [HttpGet("agents")]
    [Authorize(Policy = "ManagerOrAdmin")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AgentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAgents(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetAgentsQuery(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<AgentDto>>.Success(result, "Agents retrieved successfully."));
    }

    [HttpPost]
    [Authorize(Policy = "ManagerOrAdmin")]
    [ProducesResponseType(typeof(ApiResponse<CreateUserResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateUserRequestDto request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new CreateUserCommand(request), cancellationToken);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<CreateUserResponseDto>.Success(result, "User created successfully."));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "ManagerOrAdmin")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequestDto request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new UpdateUserCommand(id, request), cancellationToken);
        return Ok(ApiResponse<UserDto>.Success(result, "User updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await Mediator.Send(new DeleteUserCommand(id), cancellationToken);
        return Ok(ApiResponse<object>.Success(new { Id = id }, "User deleted successfully."));
    }
}
