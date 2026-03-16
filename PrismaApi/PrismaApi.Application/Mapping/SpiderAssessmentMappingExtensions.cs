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
                Value = entity.Value,
                Risk = entity.Risk,
                Cost = entity.Cost,
                Feasibility = entity.Feasibility,
                Impact = entity.Impact,
                Comment = entity.Comment,
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
                Value = dto.Value,
                Risk = dto.Risk,
                Cost = dto.Cost,
                Feasibility = dto.Feasibility,
                Impact = dto.Impact,
                Comment = dto.Comment,
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
