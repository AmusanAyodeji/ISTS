using MediatR;
using Ticketing.Application.DTOs.Users;
using Ticketing.Application.Interfaces.Persistence;

namespace Ticketing.Application.Features.Users.Commands.UpdateUser;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserDto>
{
    private readonly IUserRepository _userRepository;

    public UpdateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdWithRolesAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        user.FirstName = request.Request.FirstName;
        user.LastName = request.Request.LastName;
        user.IsActive = request.Request.IsActive;
        user.DepartmentId = request.Request.DepartmentId;

        var roles = await _userRepository.GetRolesByNamesAsync(request.Request.Roles, cancellationToken);
        if (roles.Count != request.Request.Roles.Distinct(StringComparer.OrdinalIgnoreCase).Count())
        {
            throw new InvalidOperationException("One or more roles are invalid.");
        }

        user.Roles.Clear();
        foreach (var role in roles)
        {
            user.Roles.Add(role);
        }

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            IsActive = user.IsActive,
            Roles = user.Roles.Select(x => x.Name).ToList(),
            DepartmentId = user.DepartmentId,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}
