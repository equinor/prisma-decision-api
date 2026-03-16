using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PrismaApi.Domain.Dtos
{
    public class SpiderAssessmentDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }
        [JsonPropertyName("value")]
        public int Value { get; set; }
        [JsonPropertyName("risk")]
        public int Risk { get; set; }
        [JsonPropertyName("cost")]
        public int Cost { get; set; }
        [JsonPropertyName("feasibility")]
        public int Feasibility { get; set; }
        [JsonPropertyName("impact")]
        public int Impact { get; set; }
        [JsonPropertyName("comment")]
        public string Comment { get; set; } = string.Empty;
        [JsonPropertyName("assessment_id")]
        public Guid AssessmentId { get; set; }

    }

    public class SpiderAssessmentIncomingDto : SpiderAssessmentDto
    {
    }
    public class SpiderAssessmentOutgoingDto : SpiderAssessmentDto
    { }

}
