using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Ticketing.Application.DTOs.Users;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Domain.Constants;

namespace Ticketing.Application.Features.Users.Queries.GetAgents;

public class GetAgentsQueryHandler : IRequestHandler<GetAgentsQuery, IReadOnlyList<AgentDto>>
{
    private readonly IUserRepository _userRepository;

    public GetAgentsQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IReadOnlyList<AgentDto>> Handle(GetAgentsQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.ListWithRolesAsync(cancellationToken);

        var agents = users
            .Where(u => u.IsActive && u.Roles.Any(r =>
                r.Name == SystemRoles.Agent ||
                r.Name == SystemRoles.Manager ||
                r.Name == SystemRoles.Admin))
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .Select(u => new AgentDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                FullName = $"{u.FirstName} {u.LastName}",
                Email = u.Email,
                Initials = $"{u.FirstName[0]}{u.LastName[0]}"
            })
            .ToList();

        return agents;
    }
}
