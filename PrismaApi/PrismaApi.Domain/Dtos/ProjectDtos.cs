using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PrismaApi.Domain.Dtos;

public class ProjectDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; } = Guid.NewGuid();
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("opportunity_statement")]
    public string OpportunityStatement { get; set; } = string.Empty;
    [JsonPropertyName("parent_project_id")]
    public Guid? ParentProjectId { get; set; }
    [JsonPropertyName("parent_project_name")]
    public string? ParentProjectName { get; set; }
    [JsonPropertyName("public")]
    public bool Public { get; set; }
    [JsonPropertyName("end_date")]
    public DateTimeOffset EndDate { get; set; } = DateTimeOffset.UtcNow.AddDays(30);
}

public class ProjectCreateDto : ProjectDto
{
    [JsonPropertyName("objectives")]
    public List<ObjectiveViaProjectDto> Objectives { get; set; } = new();
    [JsonPropertyName("users")]
    public List<ProjectRoleCreateDto> Users { get; set; } = new();
}

public class ProjectIncomingDto : ProjectDto
{
    [JsonPropertyName("objectives")]
    public List<ObjectiveViaProjectDto> Objectives { get; set; } = new();
    [JsonPropertyName("strategies")]
    public List<StrategyIncomingDto> Strategies { get; set; } = new();
    [JsonPropertyName("users")]
    public List<ProjectRoleIncomingDto> Users { get; set; } = new();
}

public class ProjectOutgoingDto : ProjectDto
{
    [JsonPropertyName("objectives")]
    public List<ObjectiveOutgoingDto> Objectives { get; set; } = new();
    [JsonPropertyName("strategies")]
    public List<StrategyOutgoingDto> Strategies { get; set; } = new();
    [JsonPropertyName("users")]
    public List<ProjectRoleOutgoingDto> Users { get; set; } = new();
}

public class PopulatedProjectDto : ProjectDto
{
    [JsonPropertyName("objectives")]
    public List<ObjectiveOutgoingDto> Objectives { get; set; } = new();
    [JsonPropertyName("strategies")]
    public List<StrategyOutgoingDto> Strategies { get; set; } = new();
    [JsonPropertyName("users")]
    public List<ProjectRoleOutgoingDto> Users { get; set; } = new();
}
