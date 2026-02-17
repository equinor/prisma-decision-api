using PrismaApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrismaApi.Domain.Entities;

public class Project : AuditableEntity, IBaseEntity<Guid>
{
    [Column("id")]
    public Guid Id { get; set; }
    [Column("name")]
    public string Name { get; set; } = string.Empty;
    [Column("parent_project_id")]
    public Guid? ParentProjectId { get; set; }
    [Column("parent_project_name")]
    public string? ParentProjectName { get; set; }
    [Column("opportunity_statement")]
    public string OpportunityStatement { get; set; } = string.Empty;
    [Column("public")]
    public bool Public { get; set; }
    [Column("end_date")]
    public DateTimeOffset EndDate { get; set; } = DateTimeOffset.UtcNow.AddDays(30);

    public ICollection<ProjectRole> ProjectRoles { get; set; } = new List<ProjectRole>();
    public ICollection<Objective> Objectives { get; set; } = new List<Objective>();
    public ICollection<Strategy> Strategies { get; set; } = new List<Strategy>();
    public ICollection<Issue> Issues { get; set; } = new List<Issue>();
    public ICollection<Node> Nodes { get; set; } = new List<Node>();
    public ICollection<Edge> Edges { get; set; } = new List<Edge>();
}
