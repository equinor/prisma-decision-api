using System;
using System.Text.Json.Serialization;

namespace PrismaApi.Domain.Dtos;

public class BoardNodeDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }
    [JsonPropertyName("project_id")]
    public Guid ProjectId { get; set; }
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
    [JsonPropertyName("height")]
    public double Height { get; set; }
    [JsonPropertyName("width")]
    public double Width { get; set; }
    [JsonPropertyName("x_position")]
    public double XPosition { get; set; }
    [JsonPropertyName("y_position")]
    public double YPosition { get; set; }
    [JsonPropertyName("rotation")]
    public double Rotation { get; set; }
    [JsonPropertyName("data")]
    public string Data { get; set; } = string.Empty;
    [JsonPropertyName("color")]
    public string Color { get; set; } = string.Empty;
}

public class BoardNodeIncomingDto : BoardNodeDto
{
}

public class BoardNodeOutgoingDto : BoardNodeDto
{
}
