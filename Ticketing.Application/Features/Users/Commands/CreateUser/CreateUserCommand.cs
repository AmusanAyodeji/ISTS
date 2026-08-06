using MediatR;
using Ticketing.Application.DTOs.Users;

namespace Ticketing.Application.Features.Users.Commands.CreateUser;

public record CreateUserCommand(CreateUserRequestDto Request) : IRequest<CreateUserResponseDto>;
