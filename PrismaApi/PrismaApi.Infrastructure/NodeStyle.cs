using System;
using System.Collections.Generic;

namespace PrismaApi.Infrastructure;

public partial class NodeStyle
{
    public Guid Id { get; set; }

    public Guid NodeId { get; set; }

    public double XPosition { get; set; }

    public double YPosition { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public virtual Node Node { get; set; } = null!;
}
