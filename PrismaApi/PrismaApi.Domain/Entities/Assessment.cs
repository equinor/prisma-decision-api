using PrismaApi.Domain.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrismaApi.Domain.Entities;

public class Assessment : AuditableEntity, IBaseEntity<Guid>
{
    [Column("id")]
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public Guid ProjectId { get; set; }

    public bool IsCompleted { get; set; } = false;
    public Project? Project { get; set; }
    public ICollection<DecisionQualityAssessment> DecisionQualityAssessments { get; set; } = new List<DecisionQualityAssessment>();
}
