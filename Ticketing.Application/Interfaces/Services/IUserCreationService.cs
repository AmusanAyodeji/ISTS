using Ticketing.Application.DTOs.Users;

namespace Ticketing.Application.Interfaces.Services;

public interface IUserCreationService
{
    Task<CreateUserResponseDto> CreateUser(CreateUserRequestDto userdto, CancellationToken cancellationToken);
}