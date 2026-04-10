using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Mapping
{
    public static class AssessmentMappingExtensions
    {

        public static AssessmentOutgoingDto ToOutgoingDto(this Assessment entity)
        {
            return new AssessmentOutgoingDto
            {
                Id = entity.Id,
                Name = entity.Name,
                ProjectId = entity.ProjectId,
                IsCompleted = entity.IsCompleted,
                DecisionQualityAssessments = entity.DecisionQualityAssessments?.Select(sa => sa.ToOutgoingDto()).ToList() ?? new()
            };
        }

        public static List<AssessmentOutgoingDto> ToOutgoingDtos(this IEnumerable<Assessment> entities)
        {
            return entities.Select(ToOutgoingDto).ToList();
        }

        public static Assessment ToEntity(this AssessmentIncomingDto dto, UserOutgoingDto userDto)
        {
            return new Assessment
            {
                Id = dto.Id,
                Name = dto.Name,
                ProjectId = dto.ProjectId,
                IsCompleted = dto.IsCompleted,
                CreatedById = userDto.Id, // Set the CreatedById from the userDto
                UpdatedById = userDto.Id  // Set the UpdatedById from the userDto
            };
        }
        public static List<Assessment> ToEntities(this List<AssessmentIncomingDto> dtos, UserOutgoingDto userDto)
        {
            return dtos.Select(dto => dto.ToEntity(userDto)).ToList();
        }

    };
}
