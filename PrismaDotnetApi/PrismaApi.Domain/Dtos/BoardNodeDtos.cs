using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using PrismaApi.Domain.Constants;

namespace PrismaApi.Domain.Dtos;

public interface ITypedBoardNode
{
    string Type { get; }
}

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
    [JsonPropertyName("stroke_width")]
    public float StrokeWidth { get; set; } = 8;
    
}

public class BoardNodeIncomingDto : BoardNodeDto, ITypedBoardNode
{
    [JsonPropertyName("type")]
    [EnumDataType(typeof(BoardNodeTypes), ErrorMessage = "Invalid Type")]
    public required string Type { get; set; }
    [JsonPropertyName("stroke_style")]
    [EnumDataType(typeof(BoardNodeStrokeStyles), ErrorMessage = "Invalid StrokeStyle")]
    public string StrokeStyle { get; set; } = BoardNodeStrokeStyles.Solid.ToString();
    [JsonPropertyName("opacity")]
    [Range(0, 100, ErrorMessage = "Opacity must be between 0 and 100")]
    public int Opacity { get; set; } = 100;
    [JsonPropertyName("text_size")]
    [Range(1, int.MaxValue, ErrorMessage = "TextSize must be a positive integer")]
    public int TextSize { get; set; } = 24;
}

public class BoardNodeOutgoingDto : BoardNodeDto, ITypedBoardNode
{
    [JsonPropertyName("type")]
    public required string Type { get; set; }
    [JsonPropertyName("stroke_style")]
    public required string StrokeStyle { get; set; }
    [JsonPropertyName("opacity")]
    public int Opacity { get; set; } = 100;
    [JsonPropertyName("text_size")]
    public int TextSize { get; set; } = 24;
}
