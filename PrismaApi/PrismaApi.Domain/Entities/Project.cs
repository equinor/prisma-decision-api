using PrismaApi.Domain.Interfaces;
using System;
using System.Collections.Generic;

namespace PrismaApi.Domain.Entities;

public class Project : AuditableEntity, IBaseEntity<Guid>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid? ParentProjectId { get; set; }
    public string? ParentProjectName { get; set; }
    public string OpportunityStatement { get; set; } = string.Empty;
    public bool Public { get; set; }
    public DateTime EndDate { get; set; } = DateTime.UtcNow.AddDays(30);

    public ICollection<ProjectRole> ProjectRoles { get; set; } = new List<ProjectRole>();
    public ICollection<Objective> Objectives { get; set; } = new List<Objective>();
    public ICollection<Strategy> Strategies { get; set; } = new List<Strategy>();
    public ICollection<Issue> Issues { get; set; } = new List<Issue>();
    public ICollection<Node> Nodes { get; set; } = new List<Node>();
    public ICollection<Edge> Edges { get; set; } = new List<Edge>();
}
