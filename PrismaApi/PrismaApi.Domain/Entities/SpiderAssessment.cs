using PrismaApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrismaApi.Domain.Entities;

public class SpiderAssessment : AuditableEntity, IBaseEntity<Guid>
{
    [Column("id")]
    public Guid Id { get; set; }
    [Column("value")]
    public int Value { get; set; }
    [Column("risk")]
    public int Risk { get; set; }
    [Column("cost")]
    public int Cost { get; set; }
    [Column("feasibility")]
    public int Feasibility { get; set; }
    [Column("impact")]
    public int Impact { get; set; }
    [Column("comment")]
    public string Comment { get; set; } = string.Empty;
    [Column("assessment_id")]
    public Guid AssessmentId { get; set; }
    public Assessment? Assessment { get; set; }

}
