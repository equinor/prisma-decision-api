using PrismaApi.Domain.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrismaApi.Domain.Entities;

public class DecisionQualityAssessment : AuditableEntity, IBaseEntity<Guid>
{
    [Column("id")]
    public Guid Id { get; set; }
    public int AppropriateFrame { get; set; }
    public int TradeOffAnalysis { get; set; }
    public int ReasoningCorrectness { get; set; }
    public int InformationReliability { get; set; }
    public int CommitmentToAction { get; set; }
    public string Comment { get; set; } = string.Empty;

    public int DoableAlternatives { get; set; }
    public Guid AssessmentId { get; set; }
    public Assessment? Assessment { get; set; }

}
