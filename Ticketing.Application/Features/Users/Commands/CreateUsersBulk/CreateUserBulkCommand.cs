using MediatR;
using Ticketing.Application.DTOs.Users;

namespace Ticketing.Application.Features.Users.Commands.CreateUsersBulk;

public record CreateUsersBulkCommand(Guid DepartmentId, IList<UserInfo> userinfo, string FileName) : IRequest<BulkResponseDTO>;