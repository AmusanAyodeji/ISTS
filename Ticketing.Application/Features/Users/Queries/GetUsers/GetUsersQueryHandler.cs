using MediatR;
using Ticketing.Application.DTOs.Users;
using Ticketing.Application.Interfaces.Persistence;

namespace Ticketing.Application.Features.Users.Queries.GetUsers;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, IReadOnlyList<UserDto>>
{
    private readonly IUserRepository _userRepository;

    public GetUsersQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IReadOnlyList<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.ListWithRolesAsync(cancellationToken);

        return users
            .Select(x => new UserDto
            {
                Id = x.Id,
                FirstName = x.FirstName,
                LastName = x.LastName,
                FullName = $"{x.FirstName} {x.LastName}",
                Email = x.Email,
                IsActive = x.IsActive,
                Roles = x.Roles.Select(r => r.Name).ToList(),
                DepartmentId = x.DepartmentId,
                DepartmentName = x.Department?.Name,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .OrderBy(x => x.FullName)
            .ToList();
    }
}
