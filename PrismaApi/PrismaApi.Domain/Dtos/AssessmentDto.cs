using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PrismaApi.Domain.Dtos
{
    public class AssessmentDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        [JsonPropertyName("is_completed")]
        public bool IsCompleted { get; set; } = false;

        [JsonPropertyName("project_id")]
        public Guid ProjectId { get; set; }

    }
    public class AssessmentIncomingDto : AssessmentDto
    {
    }
    public class AssessmentOutgoingDto : AssessmentDto

    {
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("decision_quality_assessments")]
        public List<DecisionQualityAssessmentOutgoingDto> DecisionQualityAssessments { get; set; } = new();
    }
}
