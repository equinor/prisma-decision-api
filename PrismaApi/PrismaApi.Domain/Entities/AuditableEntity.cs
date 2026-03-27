using System.ComponentModel.DataAnnotations.Schema;

namespace PrismaApi.Domain.Entities;

public abstract class AuditableEntity : BaseEntity
{
    public required string CreatedById { get; set; }
    public required string UpdatedById { get; set; }

    public User? CreatedBy { get; set; }
    public User? UpdatedBy { get; set; }
}
