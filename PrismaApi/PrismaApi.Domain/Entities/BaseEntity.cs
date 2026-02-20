using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrismaApi.Domain.Entities;

public abstract class BaseEntity
{
    [Column("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }
    [Column("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }
}
