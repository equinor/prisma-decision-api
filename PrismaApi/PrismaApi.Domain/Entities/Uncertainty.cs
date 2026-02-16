using System;
using System.Collections.Generic;

namespace PrismaApi.Domain.Entities;

public class Uncertainty : BaseEntity
{
    public Guid Id { get; set; }
    public Guid IssueId { get; set; }
    public bool IsKey { get; set; } = true;

    public Issue? Issue { get; set; }
    public ICollection<Outcome> Outcomes { get; set; } = new List<Outcome>();
}
