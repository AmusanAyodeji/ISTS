using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ticketing.Domain.Enums;

[JsonConverter(typeof(TicketStatusJsonConverter))]
public enum TicketStatus
{
    [EnumMember(Value = "Active")]
    Open,

    [EnumMember(Value = "Ongoing")]
    InProgress,

    [EnumMember(Value = "Resolved")]
    Resolved,

    [EnumMember(Value = "Closed")]
    Closed
}

public class TicketStatusJsonConverter : JsonConverter<TicketStatus>
{
    public override TicketStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrWhiteSpace(value))
            throw new JsonException("Ticket status cannot be empty.");

        foreach (var field in typeof(TicketStatus).GetFields())
        {
            if (!field.IsStatic)
                continue;

            if (string.Equals(field.Name, value, StringComparison.OrdinalIgnoreCase))
                return (TicketStatus)field.GetValue(null)!;

            var enumMember = field.GetCustomAttributes(typeof(EnumMemberAttribute), false)
                .Cast<EnumMemberAttribute>()
                .FirstOrDefault();
            if (enumMember?.Value != null && string.Equals(enumMember.Value, value, StringComparison.OrdinalIgnoreCase))
                return (TicketStatus)field.GetValue(null)!;
        }

        throw new JsonException($"Invalid ticket status: {value}.");
    }

    public override void Write(Utf8JsonWriter writer, TicketStatus value, JsonSerializerOptions options)
    {
        var field = typeof(TicketStatus).GetField(value.ToString());
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
