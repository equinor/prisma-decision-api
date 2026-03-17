using System;
using System.Collections.Generic;

namespace PrismaApi.Infrastructure;

public partial class ProjectRole
{
    public Guid Id { get; set; }

    public Guid ProjectId { get; set; }

    public int UserId { get; set; }

    public string Role { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public int CreatedById { get; set; }

    public int UpdatedById { get; set; }

    public virtual User CreatedBy { get; set; } = null!;

    public virtual Project Project { get; set; } = null!;

    public virtual User UpdatedBy { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
