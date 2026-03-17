using System;
using System.Collections.Generic;

namespace PrismaApi.Infrastructure;

public partial class Decision
{
    public Guid Id { get; set; }

    public Guid IssueId { get; set; }

    public string Type { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public virtual Issue Issue { get; set; } = null!;

    public virtual ICollection<Option> Options { get; set; } = new List<Option>();
}
