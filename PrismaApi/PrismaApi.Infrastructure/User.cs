using System;
using System.Collections.Generic;

namespace PrismaApi.Infrastructure;

public partial class User
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string AzureId { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public virtual ICollection<Issue> IssueCreatedBies { get; set; } = new List<Issue>();

    public virtual ICollection<Issue> IssueUpdatedBies { get; set; } = new List<Issue>();

    public virtual ICollection<Objective> ObjectiveCreatedBies { get; set; } = new List<Objective>();

    public virtual ICollection<Objective> ObjectiveUpdatedBies { get; set; } = new List<Objective>();

    public virtual ICollection<Project> ProjectCreatedBies { get; set; } = new List<Project>();

    public virtual ICollection<ProjectRole> ProjectRoleCreatedBies { get; set; } = new List<ProjectRole>();

    public virtual ICollection<ProjectRole> ProjectRoleUpdatedBies { get; set; } = new List<ProjectRole>();

    public virtual ICollection<ProjectRole> ProjectRoleUsers { get; set; } = new List<ProjectRole>();

    public virtual ICollection<Project> ProjectUpdatedBies { get; set; } = new List<Project>();

    public virtual ICollection<Strategy> StrategyCreatedBies { get; set; } = new List<Strategy>();

    public virtual ICollection<Strategy> StrategyUpdatedBies { get; set; } = new List<Strategy>();
}
