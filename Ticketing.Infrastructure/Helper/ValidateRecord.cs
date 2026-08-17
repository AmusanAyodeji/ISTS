using Ticketing.Application.DTOs.Users;
using Ticketing.Domain.Entities;
using Ticketing.Application.Interfaces.Services;
using Ticketing.Application.Interfaces.Persistence;

namespace Ticketing.Infrastructure.Helper;

public class ValidateRecord: IValidateRecord
{
    private readonly IJobErrorRepository _jobErrorRepository;
    private readonly IUserCreationService _userCreationService;
    private bool error = false;

    public ValidateRecord(IJobErrorRepository jobErrorRepository, IUserCreationService userCreationService)
    {
        _jobErrorRepository = jobErrorRepository;
        _userCreationService = userCreationService;

    }

    public async Task PerformValidationAndSave(IList<UserInfo> information, Guid JobId, Guid DepartmentId, string FileName, CancellationToken cancellationToken)
    {
        int rowcount = 2;
        foreach (var info in information)
        {
            error = false;
            await IsEmpty(info, JobId, DepartmentId, rowcount, FileName);
            await HasNum(info.FirstName, "FirstName", JobId, DepartmentId, rowcount, FileName);
            await HasNum(info.LastName, "LastName", JobId, DepartmentId, rowcount, FileName);
            await IsValidEmail(info.Email, JobId, DepartmentId, rowcount, FileName);
            await IsValidRole(info.Role, JobId, DepartmentId, rowcount, FileName);
            rowcount ++;
            if (!error)
            {
                await _userCreationService.CreateUser(new CreateUserRequestDto { FirstName = info.FirstName, LastName = info.LastName, Email = info.Email, Roles = new List<string>{info.Role}, DepartmentId = DepartmentId }, cancellationToken);
            }
        }
    }

    private async Task IsEmpty(UserInfo info, Guid JobId, Guid DepartmentId, int rowcount, string FileName)
    {
        foreach (var property in typeof(UserInfo).GetProperties())
        {
            var value = property.GetValue(info) as string;

            if (string.IsNullOrWhiteSpace(value))
            {
                await AddError($"{property.Name} Is empty on row {rowcount} in file {FileName}", JobId, DepartmentId);
            }
        }
    }

    private async Task HasNum(string name, string propertyname, Guid JobId, Guid DepartmentId, int rowcount, string FileName)
    {
        if (name.Any(char.IsDigit))
        {
            await AddError($"{propertyname} has a number in it on row {rowcount} in file {FileName}", JobId, DepartmentId);
        }
    }

    private async Task IsValidEmail(string email, Guid JobId, Guid DepartmentId, int rowcount, string FileName)
    {
        if (!(email.Contains("@") && email.Contains(".com", StringComparison.OrdinalIgnoreCase)))
        {
            await AddError($"Invalid email on row {rowcount} in file {FileName}", JobId, DepartmentId );
        }
    }

    private async Task IsValidRole(string role, Guid JobId, Guid DepartmentId, int rowcount, string FileName)
    {
        List<String> roles = ["MANAGER", "AGENT", "STAFF"];
        if (!roles.Contains(role.ToUpper()))
        {
            await AddError($"Invalid role on row {rowcount} in file {FileName}", JobId, DepartmentId);
        }
    }

    private async Task AddError(string message, Guid JobId, Guid DepartmentId)
    {
        await _jobErrorRepository.AddAsync(new JobError { JobId = JobId, DepartmentId = DepartmentId, Message = message });
        error = true;
    }
}