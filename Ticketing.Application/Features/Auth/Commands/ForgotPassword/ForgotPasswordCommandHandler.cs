using System.Security.Cryptography;
using MediatR;
using Microsoft.Extensions.Configuration;
using Ticketing.Application.Common.Security;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Application.Interfaces.Services;

namespace Ticketing.Application.Features.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public ForgotPasswordCommandHandler(IUserRepository userRepository, IEmailService emailService, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _emailService = emailService;
        _configuration = configuration;
    }

    public async Task<bool> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Request.Email, cancellationToken);
        if (user is null)
        {
            return true;
        }

        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
        var tokenHash = TokenHashing.Hash(token);
        var resetTokenExpiryMinutes = int.TryParse(_configuration["Jwt:PasswordResetTokenExpiryMinutes"], out var minutes) ? minutes : 30;

        user.PasswordResetTokenHash = tokenHash;
        user.PasswordResetTokenExpiresAt = DateTime.UtcNow.AddMinutes(resetTokenExpiryMinutes);
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(cancellationToken);

        var subject = "Password Reset Request";
        var body = $"Use this token to reset your password: <strong>{token}</strong>. It expires in {resetTokenExpiryMinutes} minutes.";
        await _emailService.SendAsync(user.Email, subject, body, cancellationToken);

        return true;
    }
}
