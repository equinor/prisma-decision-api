using System;
using System.Text.Json.Serialization;

namespace PrismaApi.Domain.Dtos;

public class NodeStyleDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; } = Guid.NewGuid();
    [JsonPropertyName("node_id")]
    public Guid NodeId { get; set; }
    [JsonPropertyName("x_position")]
    public double XPosition { get; set; }
    [JsonPropertyName("y_position")]
    public double YPosition { get; set; }
}

public class NodeStyleIncomingDto : NodeStyleDto
{
}

public class NodeStyleOutgoingDto : NodeStyleDto
{
}
