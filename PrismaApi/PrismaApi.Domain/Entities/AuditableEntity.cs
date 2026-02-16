using System;

namespace PrismaApi.Domain.Entities;

public abstract class AuditableEntity : BaseEntity
{
    public Guid? CreatedById { get; set; }
    public Guid? UpdatedById { get; set; }

    public User? CreatedBy { get; set; }
    public User? UpdatedBy { get; set; }
}
