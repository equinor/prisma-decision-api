using PrismaApi.Domain.Interfaces;
using System;

namespace PrismaApi.Domain.Entities;

public class Utility : BaseEntity, IBaseEntity<Guid>
{
    public Guid Id { get; set; }
    public Guid IssueId { get; set; }

    public Issue? Issue { get; set; }
}
