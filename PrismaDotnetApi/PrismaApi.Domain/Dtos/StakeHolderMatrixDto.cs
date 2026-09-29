using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace PrismaApi.Domain.Dtos
{
    public class StakeholderMatrixDto
    {
        [JsonPropertyName("stakeholder_matrix_id")]
        public Guid Id { get; set; } = Guid.NewGuid();
        [JsonPropertyName("project_id")]
        public Guid ProjectId { get; set; }
        [JsonPropertyName("stakeholder_name")]
        public string StakeholderName { get; set; } = string.Empty;
        [JsonPropertyName("stakeholder_role")]
        public string StakeholderRole { get; set; } = string.Empty;
        [JsonPropertyName("affecting_the_decision")]
        public int AffectingTheDecision { get; set; }

        [JsonPropertyName("affected_by_the_decision")]
        public int AffectedByTheDecision { get; set; }

    }

    public class StakeholderMatrixIncomingDto : StakeholderMatrixDto
    {
    }
    public class StakeholderMatrixOutgoingDto : StakeholderMatrixDto
    {

    }
}
