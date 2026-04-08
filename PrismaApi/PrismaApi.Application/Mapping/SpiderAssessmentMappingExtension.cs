using System.Collections.Generic;
using System.Linq;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Mapping
{
    public static class SpiderAssessmentMappingExtensions
    {
        public static SpiderAssessmentOutgoingDto ToOutgoingDto(this SpiderAssessment entity)
        {
            return new SpiderAssessmentOutgoingDto
            {
                Id = entity.Id,
                AppropriateFrame = entity.AppropriateFrame,
                TradeOffAnalysis = entity.TradeOffAnalysis,
                ReasoningCorrectness = entity.ReasoningCorrectness,
                InformationReliability = entity.InformationReliability,
                CommitmentToAction = entity.CommitmentToAction,
                Comment = entity.Comment,
                DoableAlternatives = entity.DoableAlternatives,
                AssessmentId = entity.AssessmentId
            };
        }

        public static List<SpiderAssessmentOutgoingDto> ToOutgoingDtos(this IEnumerable<SpiderAssessment> entities)
        {
            return entities.Select(ToOutgoingDto).ToList();
        }

        public static SpiderAssessment ToEntity(this SpiderAssessmentIncomingDto dto, UserOutgoingDto userDto)
        {
            return new SpiderAssessment
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

        public static List<SpiderAssessment> ToEntities(this List<SpiderAssessmentIncomingDto> dtos, UserOutgoingDto userDto)
        {
            return dtos.Select(dto => dto.ToEntity(userDto)).ToList();
        }
    }
}
