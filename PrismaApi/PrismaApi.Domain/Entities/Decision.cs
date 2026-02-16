using PrismaApi.Domain.Interfaces;
using System;
using System.Collections.Generic;

namespace PrismaApi.Domain.Entities;

public class Decision : BaseEntity, IBaseEntity<Guid>
{
    public Guid Id { get; set; }
    public Guid IssueId { get; set; }
    public string Type { get; set; } = string.Empty;

    public Issue? Issue { get; set; }
    public ICollection<Option> Options { get; set; } = new List<Option>();
}
