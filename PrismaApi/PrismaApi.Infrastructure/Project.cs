using System;
using System.Collections.Generic;

namespace PrismaApi.Infrastructure;

public partial class Project
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public Guid? ParentProjectId { get; set; }

    public string? ParentProjectName { get; set; }

    public string OpportunityStatement { get; set; } = null!;

    public bool Public { get; set; }

    public DateTimeOffset EndDate { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public int CreatedById { get; set; }

    public int UpdatedById { get; set; }

    public virtual User CreatedBy { get; set; } = null!;

    public virtual ICollection<Edge> Edges { get; set; } = new List<Edge>();

    public virtual ICollection<Issue> Issues { get; set; } = new List<Issue>();

    public virtual ICollection<Node> Nodes { get; set; } = new List<Node>();

    public virtual ICollection<Objective> Objectives { get; set; } = new List<Objective>();

    public virtual ICollection<ProjectRole> ProjectRoles { get; set; } = new List<ProjectRole>();

    public virtual ICollection<Strategy> Strategies { get; set; } = new List<Strategy>();

    public virtual User UpdatedBy { get; set; } = null!;
}
