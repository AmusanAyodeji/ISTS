using MediatR;
using Ticketing.Application.DTOs.Auth;

namespace Ticketing.Application.Features.Auth.Commands.ResetPassword;

public record ResetPasswordCommand(ResetPasswordRequestDto Request) : IRequest<bool>;
