using Ticketing.Application.DTOs.Users;

namespace Ticketing.Application.Interfaces.Services;

public interface IValidateRecord
{
    Task PerformValidationAndSave(IList<UserInfo> information, Guid JobId, Guid DepartmentId, string FileName, CancellationToken cancellationToken);
}