using MediatR;
using Ticketing.Application.DTOs.Users;

namespace Ticketing.Application.Features.Users.Queries.GetCurrentUser;

public record GetCurrentUserQuery : IRequest<CurrentUserDto>;
