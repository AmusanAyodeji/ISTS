using MediatR;
using Ticketing.Application.DTOs.Auth;

namespace Ticketing.Application.Features.Auth.Commands.Register;

public record RegisterCommand(RegisterRequestDto Request) : IRequest<AuthTokenResponseDto>;
