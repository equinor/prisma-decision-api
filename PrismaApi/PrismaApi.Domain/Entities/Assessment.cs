using PrismaApi.Domain.Interfaces;
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

    [Column("is_assessment_completed")]
    public bool IsAssessmentCompleted { get; set; } = false;
    public Project? Project { get; set; }
    public ICollection<DecisionQualityAssessment> DecisionQualityAssessments { get; set; } = new List<DecisionQualityAssessment>();
}
