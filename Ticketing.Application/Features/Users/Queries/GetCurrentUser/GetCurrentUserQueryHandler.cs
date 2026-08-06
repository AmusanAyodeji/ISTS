using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Ticketing.Application.DTOs.Users;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Application.Interfaces.Services;

namespace Ticketing.Application.Features.Users.Queries.GetCurrentUser;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, CurrentUserDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetCurrentUserQueryHandler(IUserRepository userRepository, ICurrentUserService currentUserService)
    {
        _userRepository = userRepository;
        _currentUserService = currentUserService;
    }

    public async Task<CurrentUserDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var user = await _userRepository.GetByIdWithRolesAsync(userId.Value, cancellationToken);
        if (user == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        var roles = user.Roles.Select(r => r.Name).ToList();

        return new CurrentUserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            FullName = $"{user.FirstName} {user.LastName}",
            Roles = roles,
            DepartmentId = user.DepartmentId,
            DepartmentName = user.Department?.Name
        };
    }
}
