using MediatR;
using Ticketing.Application.DTOs.Users;

namespace Ticketing.Application.Features.Users.Queries.GetErrors;

public record GetErrorsQuery(Guid JobId) : IRequest<List<JobErrorDTO>>;
