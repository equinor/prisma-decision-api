using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PrismaApi.Domain.Dtos
{
    public class DecisionQualityAssessmentDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; } = Guid.NewGuid();
        [JsonPropertyName("appropriate_frame")]
        public int AppropriateFrame { get; set; }
        [JsonPropertyName("trade_off_analysis")]
        public int TradeOffAnalysis { get; set; }
        [JsonPropertyName("reasoning_correctness")]
        public int ReasoningCorrectness { get; set; }
        [JsonPropertyName("information_reliability")]
        public int InformationReliability { get; set; }
        [JsonPropertyName("commitment_to_action")]
        public int CommitmentToAction { get; set; }
        [JsonPropertyName("comment")]
        public string Comment { get; set; } = string.Empty;
        [JsonPropertyName("doable_alternatives")]
        public int DoableAlternatives { get; set; }
        [JsonPropertyName("assessment_id")]
        public Guid AssessmentId { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt { get; set; }

    }

    public class DecisionQualityAssessmentIncomingDto : DecisionQualityAssessmentDto
    {
    }
    public class DecisionQualityAssessmentOutgoingDto : DecisionQualityAssessmentDto
    {
        [JsonPropertyName("created_by_id")]
        public string CreatedById { get; set; } = string.Empty;
    }

}
