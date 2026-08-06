using System.Reflection;
using System.Runtime.Serialization;

namespace Ticketing.Application.Common.Mappings;

public static class EnumDisplayMapper
{
    public static string GetDisplayName<TEnum>(this TEnum value) where TEnum : struct, Enum
    {
        var memberInfo = typeof(TEnum).GetMember(value.ToString()).FirstOrDefault();
        if (memberInfo != null)
        {
            var attribute = memberInfo.GetCustomAttribute<EnumMemberAttribute>();
            if (attribute != null && !string.IsNullOrWhiteSpace(attribute.Value))
            {
                return attribute.Value;
            }
        }

        return value.ToString();
    }
}
