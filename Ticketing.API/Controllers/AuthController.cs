using Microsoft.AspNetCore.Mvc;
using Ticketing.API.Common.Models;
using Ticketing.Application.DTOs.Auth;
using Ticketing.Application.Features.Auth.Commands.ForgotPassword;
using Ticketing.Application.Features.Auth.Commands.Login;
using Ticketing.Application.Features.Auth.Commands.ResetPassword;

namespace Ticketing.API.Controllers;

public class AuthController : BaseApiController
{
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthTokenResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new LoginCommand(request), cancellationToken);
        return Ok(ApiResponse<AuthTokenResponseDto>.Success(result, "Login successful."));
    }

    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request, CancellationToken cancellationToken)
    {
        await Mediator.Send(new ForgotPasswordCommand(request), cancellationToken);
        return Ok(ApiResponse<object>.Success(new { }, "If the account exists, a reset token has been sent."));
    }

    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request, CancellationToken cancellationToken)
    {
        await Mediator.Send(new ResetPasswordCommand(request), cancellationToken);
        return Ok(ApiResponse<object>.Success(new { }, "Password has been reset successfully."));
    }
}
