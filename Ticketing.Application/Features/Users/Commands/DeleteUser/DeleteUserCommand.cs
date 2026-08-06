using MediatR;

namespace Ticketing.Application.Features.Users.Commands.DeleteUser;

public record DeleteUserCommand(Guid UserId) : IRequest<bool>;
