using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Ticketing.Application.DTOs.Users;
using Ticketing.Application.Interfaces.Services;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Domain.Constants;
using Ticketing.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Ticketing.Infrastructure.Helper;

public class UserCreationService : IUserCreationService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IEmailService _emailService;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly ILogger<UserCreationService> _logger;

    public UserCreationService(IUserRepository userRepository, IMapper mapper, IPasswordHasher<User> passwordHasher, IEmailService emailService, ILogger<UserCreationService> logger)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _passwordHasher = passwordHasher;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<CreateUserResponseDto> CreateUser(CreateUserRequestDto userdto, CancellationToken cancellationToken)
    {
        var existingUser = await _userRepository.GetByEmailAsync(userdto.Email, cancellationToken);
        if (existingUser is not null)
        {
            throw new InvalidOperationException($"A user with this email already exists. email: {existingUser.Email}");
        }

        var user = _mapper.Map<User>(userdto);
        var rndpw = generateRandomPassword();
        user.PasswordHash = _passwordHasher.HashPassword(user, rndpw);

        if (userdto.DepartmentId.HasValue)
        {
            var departmentExists = await _userRepository.DepartmentExistsAsync(userdto.DepartmentId.Value, cancellationToken);
            if (!departmentExists)
            {
                throw new ValidationException(new[]
                {
                    new ValidationFailure("Request.DepartmentId", "DepartmentId does not exist.")
                });
            }
        }

        var requestedRoles = userdto.Roles.Count == 0
            ? new[] { SystemRoles.Staff }
            : userdto.Roles
                .Select(role => SystemRoles.All.FirstOrDefault(r => r.Equals(role, StringComparison.OrdinalIgnoreCase)) ?? role)
                .ToArray();

        var roles = await _userRepository.GetRolesByNamesAsync(requestedRoles, cancellationToken);
        if (roles.Count != requestedRoles.Distinct(StringComparer.OrdinalIgnoreCase).Count())
        {
            throw new InvalidOperationException("One or more roles are invalid.");
        }

        foreach (var role in roles)
        {
            user.Roles.Add(role);
        }

        await _userRepository.AddAsync(user, cancellationToken);

        try
        {
            await _emailService.SendAsync(
                userdto.Email,
                $"Account Creation For {user.FirstName} {user.LastName}",
                $"<p>An account has been created for you on ISTS</strong>:</p><p><em>\n\nUsername</em>: {user.Email}</p><p><em>\n\nPassword</em>: {rndpw}</p>",
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                $"Failed to send notification email to {user.Email} for account creation.");
        }
        return _mapper.Map<CreateUserResponseDto>(user);
    }

    private String generateRandomPassword()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        char[] randomChars = Random.Shared.GetItems(chars.ToCharArray(), 16);
        return new string(randomChars);
    }
}