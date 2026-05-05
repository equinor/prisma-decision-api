using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using PrismaApi.Domain.Constants;

namespace PrismaApi.Domain.Dtos;

public class BoardNodeDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }
    [JsonPropertyName("project_id")]
    public Guid ProjectId { get; set; }
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
    [JsonPropertyName("type")]
    [EnumDataType(typeof(BoardNodeTypes), ErrorMessage = "Invalid Type")]
    public required string Type { get; set; }
}

public class BoardNodeOutgoingDto : BoardNodeDto
{
    [JsonPropertyName("type")]
    public required string Type { get; set; }
}
