using PrismaApi.Domain.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrismaApi.Domain.Entities;

public class DecisionQualityAssessment : AuditableEntity, IBaseEntity<Guid>
{
    [Column("id")]
    public Guid Id { get; set; }
    [Column("appropriate_frame")]
    public int AppropriateFrame { get; set; }
    [Column("trade_off_analysis")]
    public int TradeOffAnalysis { get; set; }
    [Column("reasoning_correctness")]
    public int ReasoningCorrectness { get; set; }
    [Column("information_reliability")]
    public int InformationReliability { get; set; }
    [Column("commitment_to_action")]
    public int CommitmentToAction { get; set; }
    [Column("comment")]
    public string Comment { get; set; } = string.Empty;

    [Column("doable_alternatives")]
    public int DoableAlternatives { get; set; }
    [Column("assessment_id")]
    public Guid AssessmentId { get; set; }
    public Assessment? Assessment { get; set; }

}
