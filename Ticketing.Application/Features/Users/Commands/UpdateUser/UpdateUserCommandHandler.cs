using MediatR;
using Ticketing.Application.DTOs.Users;
using Ticketing.Application.Interfaces.Services;
using Ticketing.Application.Interfaces.Persistence;

namespace Ticketing.Application.Features.Users.Commands.UpdateUser;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;

    public UpdateUserCommandHandler(IUserRepository userRepository, ICurrentUserService currentUserService)
    {
        _userRepository = userRepository;
        _currentUserService = currentUserService;
    }

    public async Task<UserDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdWithRolesAsync(request.UserId, cancellationToken);
        var loggedinuser = await _userRepository.GetByIdWithRolesAsync(_currentUserService.UserId.Value, cancellationToken);
        if (user is null)
        {
            throw new KeyNotFoundException("User not found.");
        }
        foreach(var role in loggedinuser.Roles)
        {
            Console.WriteLine(role.Name);
        }
        Console.WriteLine(loggedinuser.Roles.Any(r => r.Name != "Admin"));
        if(request.UserId != _currentUserService.UserId) 
        {
            if(loggedinuser.Roles.Any(r => r.Name != "Admin")) throw new InvalidOperationException("Only the account owner or an admin can edit information");
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
