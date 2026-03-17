using System;
using System.Collections.Generic;

namespace PrismaApi.Infrastructure;

public partial class Edge
{
    public Guid Id { get; set; }

    public Guid TailId { get; set; }

    public Guid HeadId { get; set; }

    public Guid ProjectId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public virtual Node Head { get; set; } = null!;

    public virtual Project Project { get; set; } = null!;

    public virtual Node Tail { get; set; } = null!;
}
