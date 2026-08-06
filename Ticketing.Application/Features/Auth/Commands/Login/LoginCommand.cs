using MediatR;
using Ticketing.Application.DTOs.Auth;

namespace Ticketing.Application.Features.Auth.Commands.Login;

public record LoginCommand(LoginRequestDto Request) : IRequest<AuthTokenResponseDto>;
