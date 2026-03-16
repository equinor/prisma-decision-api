using PrismaApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrismaApi.Domain.Entities;

public class Assessment : AuditableEntity, IBaseEntity<Guid>
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("project_id")]
    public Guid ProjectId { get; set; }
    public Project? Project { get; set; }
    public ICollection<SpiderAssessment> SpiderAssessments { get; set; } = new List<SpiderAssessment>();
}
