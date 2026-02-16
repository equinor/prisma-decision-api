using PrismaApi.Domain.Entities;

namespace PrismaApi.Domain.Entities;

public abstract class AuditableEntity : BaseEntity
{
    public int? CreatedById { get; set; }
    public int? UpdatedById { get; set; }

    public User? CreatedBy { get; set; }
    public User? UpdatedBy { get; set; }
}
