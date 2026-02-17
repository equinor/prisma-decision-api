using PrismaApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrismaApi.Domain.Entities;

public class Decision : BaseEntity, IBaseEntity<Guid>
{
    [Column("id")]
    public Guid Id { get; set; }
    [Column("issue_id")]
    public Guid IssueId { get; set; }
    [Column("type")]
    public string Type { get; set; } = string.Empty;

    public Issue? Issue { get; set; }
    public ICollection<Option> Options { get; set; } = new List<Option>();
}
