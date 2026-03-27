using System.Text.Json;

namespace PrismaApi.Api.Configuration.JsonResponseOptions;

public class DateTimeOffsetJsonConverter : System.Text.Json.Serialization.JsonConverter<DateTimeOffset>
{
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => DateTimeOffset.Parse(reader.GetString() ?? "");

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.ffffff'Z'"));
}
