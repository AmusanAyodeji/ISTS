using CsvHelper.Configuration;
using Ticketing.Application.DTOs.Users;

namespace Ticketing.Application.Common.Mappings;

public sealed class UserInfoMap : ClassMap<UserInfo>
{
    public UserInfoMap()
    {
        Map(m => m.FirstName).Name("FirstName");
        Map(m => m.LastName).Name("LastName");
        Map(m => m.Email).Name("Email");
        Map(m => m.Role).Name("Role");
    }
}