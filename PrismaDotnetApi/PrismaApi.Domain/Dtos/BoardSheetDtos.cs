using System.Text.Json.Serialization;

namespace PrismaApi.Domain.Dtos;

public class BoardSheetDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }
    [JsonPropertyName("project_id")]
    public Guid ProjectId { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class BoardSheetIncomingDto : BoardSheetDto
{
}

public class BoardSheetOutgoingDto : BoardSheetDto
{
}
