using MediatR;
using Microsoft.AspNetCore.Identity;
using Ticketing.Application.Common.Security;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Domain.Entities;

namespace Ticketing.Application.Features.Auth.Commands.ResetPassword;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<User> _passwordHasher;

    public ResetPasswordCommandHandler(IUserRepository userRepository, IPasswordHasher<User> passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<bool> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var resetTokenHash = TokenHashing.Hash(request.Request.ResetToken);
        var user = await _userRepository.GetByPasswordResetTokenHashAsync(resetTokenHash, cancellationToken);

        if (user is null || user.PasswordResetTokenExpiresAt is null || user.PasswordResetTokenExpiresAt <= DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Invalid or expired reset token.");
        }

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Request.NewPassword);
        user.PasswordResetTokenHash = null;
        user.PasswordResetTokenExpiresAt = null;

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
