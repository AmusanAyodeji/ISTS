using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Ticketing.Application.DTOs.Auth;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Application.Interfaces.Services;
using Ticketing.Domain.Constants;
using Ticketing.Domain.Entities;
using FluentValidation;
using FluentValidation.Results;

namespace Ticketing.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthTokenResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IJwtService _jwtService;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IMapper mapper,
        IPasswordHasher<User> passwordHasher,
        IJwtService jwtService)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<AuthTokenResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Request.Email, cancellationToken);
        if (existingUser is not null)
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        var user = _mapper.Map<User>(request.Request);
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Request.Password);

        if (request.Request.DepartmentId.HasValue)
        {
            var departmentExists = await _userRepository.DepartmentExistsAsync(request.Request.DepartmentId.Value, cancellationToken);
            if (!departmentExists)
            {
                throw new ValidationException(new[]
                {
                    new ValidationFailure("Request.DepartmentId", "DepartmentId does not exist.")
                });
            }
        }

        var staffRole = await _userRepository.GetRolesByNamesAsync(new[] { SystemRoles.Staff }, cancellationToken);
        if (staffRole.Count == 0)
        {
            throw new InvalidOperationException("Default staff role is not available.");
        }

        foreach (var role in staffRole)
        {
            user.Roles.Add(role);
        }

        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return new AuthTokenResponseDto
        {
            AccessToken = _jwtService.GenerateToken(user, user.Roles.Select(x => x.Name)),
            AccessTokenExpiresAt = _jwtService.GetAccessTokenExpiryUtc()
        };
    }
}
