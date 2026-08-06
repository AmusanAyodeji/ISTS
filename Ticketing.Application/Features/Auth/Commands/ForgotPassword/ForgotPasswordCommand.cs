using MediatR;
using Ticketing.Application.DTOs.Auth;

namespace Ticketing.Application.Features.Auth.Commands.ForgotPassword;

public record ForgotPasswordCommand(ForgotPasswordRequestDto Request) : IRequest<bool>;
