using PrismaApi.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrismaApi.Domain.Entities;

public abstract class AuditableEntity : BaseEntity
{
    [Column("created_by_id")]
    public required int CreatedById { get; set; }
    [Column("updated_by_id")]
    public required int UpdatedById { get; set; }

    public User? CreatedBy { get; set; }
    public User? UpdatedBy { get; set; }
}
