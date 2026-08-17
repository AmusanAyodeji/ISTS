namespace Ticketing.Application.DTOs.Users;

public class BulkResponseDTO
{
    public Guid JobId { get; set; }
    public Guid DepartmentId { get; set; }
    public string Status { get; set; }
}

public class UserInfo
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
}

public class BulkStatusResponseDTO:BulkResponseDTO
{

}

public class BulkJobDTO
{
    public Guid JobId { get; set; }
    public Guid UserId { get; set; }
    public Guid DepartmentId { get; set; }
    public IList<UserInfo> Userdata { get; set; }
    public int TotalRows { get; set; }
    public string FileName { get; set; }
}
