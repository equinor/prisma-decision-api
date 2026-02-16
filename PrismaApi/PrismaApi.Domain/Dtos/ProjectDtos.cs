using System;
using System.Collections.Generic;

namespace PrismaApi.Domain.Dtos;

public class ProjectDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string OpportunityStatement { get; set; } = string.Empty;
    public Guid? ParentProjectId { get; set; }
    public string? ParentProjectName { get; set; }
    public bool Public { get; set; }
    public DateTime EndDate { get; set; } = DateTime.UtcNow.AddDays(30);
}

public class ProjectCreateDto : ProjectDto
{
    public List<ObjectiveViaProjectDto> Objectives { get; set; } = new();
    public List<ProjectRoleCreateDto> Users { get; set; } = new();
}

public class ProjectIncomingDto : ProjectDto
{
    public List<ObjectiveViaProjectDto> Objectives { get; set; } = new();
    public List<StrategyIncomingDto> Strategies { get; set; } = new();
    public List<ProjectRoleIncomingDto> Users { get; set; } = new();
}

public class ProjectOutgoingDto : ProjectDto
{
    public List<ObjectiveOutgoingDto> Objectives { get; set; } = new();
    public List<StrategyOutgoingDto> Strategies { get; set; } = new();
    public List<ProjectRoleOutgoingDto> Users { get; set; } = new();
}

public class PopulatedProjectDto : ProjectDto
{
    public List<ObjectiveOutgoingDto> Objectives { get; set; } = new();
    public List<StrategyOutgoingDto> Strategies { get; set; } = new();
    public List<ProjectRoleOutgoingDto> Users { get; set; } = new();
}
