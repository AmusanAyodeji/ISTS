using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ticketing.Domain.Enums;

[JsonConverter(typeof(TicketPriorityJsonConverter))]
public enum TicketPriority
{
    [EnumMember(Value = "Low")]
    Low = 1,

    [EnumMember(Value = "Medium")]
    Medium = 2,

    [EnumMember(Value = "High")]
    High = 3,

    [EnumMember(Value = "Urgent")]
    Urgent = 4
}

public class TicketPriorityJsonConverter : JsonConverter<TicketPriority>
{
    public override TicketPriority Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrWhiteSpace(value))
            throw new JsonException("Ticket priority cannot be empty.");

        foreach (var field in typeof(TicketPriority).GetFields())
        {
            if (!field.IsStatic)
                continue;

            if (string.Equals(field.Name, value, StringComparison.OrdinalIgnoreCase))
                return (TicketPriority)field.GetValue(null)!;

            var enumMember = field.GetCustomAttributes(typeof(EnumMemberAttribute), false)
                .Cast<EnumMemberAttribute>()
                .FirstOrDefault();
            if (enumMember?.Value != null && string.Equals(enumMember.Value, value, StringComparison.OrdinalIgnoreCase))
                return (TicketPriority)field.GetValue(null)!;
        }

        throw new JsonException($"Invalid ticket priority: {value}.");
    }

    public override void Write(Utf8JsonWriter writer, TicketPriority value, JsonSerializerOptions options)
    {
        var field = typeof(TicketPriority).GetField(value.ToString());
        if (field != null)
        {
            var enumMember = field.GetCustomAttributes(typeof(EnumMemberAttribute), false)
                .Cast<EnumMemberAttribute>()
                .FirstOrDefault();
            if (enumMember?.Value != null)
            {
                writer.WriteStringValue(enumMember.Value);
                return;
            }
        }

        writer.WriteStringValue(value.ToString());
    }
}
