using MediatR;
using Ticketing.Application.DTOs.Users;

namespace Ticketing.Application.Features.Users.Commands.UpdateUser;

public record UpdateUserCommand(Guid UserId, UpdateUserRequestDto Request) : IRequest<UserDto>;
