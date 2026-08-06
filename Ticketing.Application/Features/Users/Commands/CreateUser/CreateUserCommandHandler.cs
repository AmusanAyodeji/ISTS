using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Ticketing.Application.DTOs.Users;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Domain.Constants;
using Ticketing.Domain.Entities;

namespace Ticketing.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, CreateUserResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IPasswordHasher<User> _passwordHasher;

    public CreateUserCommandHandler(IUserRepository userRepository, IMapper mapper, IPasswordHasher<User> passwordHasher)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _passwordHasher = passwordHasher;
    }

    public async Task<CreateUserResponseDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
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

        var requestedRoles = request.Request.Roles.Count == 0
            ? new[] { SystemRoles.Staff }
            : request.Request.Roles;

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
        await _userRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CreateUserResponseDto>(user);
    }
}
