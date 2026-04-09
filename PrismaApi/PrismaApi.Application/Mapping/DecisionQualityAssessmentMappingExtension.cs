using System.Collections.Generic;
using System.Linq;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Mapping
{
    public static class DecisionQualityAssessmentMappingExtensions
    {
        public static DecisionQualityAssessmentOutgoingDto ToOutgoingDto(this DecisionQualityAssessment entity)
        {
            return new DecisionQualityAssessmentOutgoingDto
            {
                Id = entity.Id,
                AppropriateFrame = entity.AppropriateFrame,
                TradeOffAnalysis = entity.TradeOffAnalysis,
                ReasoningCorrectness = entity.ReasoningCorrectness,
                InformationReliability = entity.InformationReliability,
                CommitmentToAction = entity.CommitmentToAction,
                Comment = entity.Comment,
                DoableAlternatives = entity.DoableAlternatives,
                AssessmentId = entity.AssessmentId,
                CreatedAt = entity.CreatedAt.UtcDateTime,
                UpdatedAt = entity.UpdatedAt.UtcDateTime
            };
        }

        public static List<DecisionQualityAssessmentOutgoingDto> ToOutgoingDtos(this IEnumerable<DecisionQualityAssessment> entities)
        {
            return entities.Select(ToOutgoingDto).ToList();
        }

        public static DecisionQualityAssessment ToEntity(this DecisionQualityAssessmentIncomingDto dto, UserOutgoingDto userDto)
        {
            return new DecisionQualityAssessment
            {
                Id = dto.Id,
                AssessmentId = dto.AssessmentId,
                AppropriateFrame = dto.AppropriateFrame,
                TradeOffAnalysis = dto.TradeOffAnalysis,
                ReasoningCorrectness = dto.ReasoningCorrectness,
                InformationReliability = dto.InformationReliability,
                CommitmentToAction = dto.CommitmentToAction,
                Comment = dto.Comment,
                DoableAlternatives = dto.DoableAlternatives,
                CreatedById = userDto.Id,
                UpdatedById = userDto.Id
            };
        }

        public static List<DecisionQualityAssessment> ToEntities(this List<DecisionQualityAssessmentIncomingDto> dtos, UserOutgoingDto userDto)
        {
            return dtos.Select(dto => dto.ToEntity(userDto)).ToList();
        }
    }
}
