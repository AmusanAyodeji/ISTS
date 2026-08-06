using MediatR;
using Ticketing.Application.DTOs.Users;

namespace Ticketing.Application.Features.Users.Queries.GetAgents;

public record GetAgentsQuery : IRequest<IReadOnlyList<AgentDto>>;
