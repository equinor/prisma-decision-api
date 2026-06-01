using PrismaApi.Domain.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrismaApi.Domain.Entities;

public class DecisionQualityAssessment : AuditableEntity, IBaseEntity<Guid>
{
    public required Guid Id { get; set; }
    public int AppropriateFrame { get; set; }
    public int TradeOffAnalysis { get; set; }
    public int ReasoningCorrectness { get; set; }
    public int InformationReliability { get; set; }
    public int CommitmentToAction { get; set; }
    public string Comment { get; set; } = string.Empty;

    public int DoableAlternatives { get; set; }
    public required Guid AssessmentId { get; set; }
    public required Guid ProjectId { get; set; }
    public Assessment? Assessment { get; set; }
    public Project? Project { get; set; }
}
