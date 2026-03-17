using System;
using System.Collections.Generic;

namespace PrismaApi.Infrastructure;

public partial class Strategy
{
    public Guid Id { get; set; }

    public Guid ProjectId { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string Rationale { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public int CreatedById { get; set; }

    public int UpdatedById { get; set; }

    public string Icon { get; set; } = null!;

    public string IconColor { get; set; } = null!;

    public virtual User CreatedBy { get; set; } = null!;

    public virtual Project Project { get; set; } = null!;

    public virtual User UpdatedBy { get; set; } = null!;

    public virtual ICollection<Option> Options { get; set; } = new List<Option>();
}
