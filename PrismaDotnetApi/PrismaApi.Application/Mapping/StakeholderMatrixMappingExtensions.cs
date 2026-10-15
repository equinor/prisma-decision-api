using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PrismaApi.Domain.Entities;
using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Mapping
{
    public static class StakeholderMatrixMappingExtensions
    {
        public static StakeholderMatrixDto ToOutgoingDto(this StakeholderMatrixEntry entity)
        {
            return new StakeholderMatrixDto
            {
                Id = entity.Id,
                ProjectId = entity.ProjectId,
                StakeholderName = entity.StakeholderName,
                StakeholderRole = entity.StakeholderRole,
                AffectingTheDecision = entity.AffectingTheDecision,
                AffectedByTheDecision = entity.AffectedByTheDecision
            };
        }

        public static List<StakeholderMatrixDto> ToOutgoingDtos(this IEnumerable<StakeholderMatrixEntry> entities)
        {
            return entities.Select(ToOutgoingDto).ToList();
        }

        public static StakeholderMatrixEntry ToEntity(this StakeholderMatrixDto dto, UserOutgoingDto user)
        {
            return new StakeholderMatrixEntry
            {
                Id = dto.Id,
                ProjectId = dto.ProjectId,
                StakeholderName = dto.StakeholderName,
                StakeholderRole = dto.StakeholderRole,
                AffectingTheDecision = dto.AffectingTheDecision,
                AffectedByTheDecision = dto.AffectedByTheDecision,
                CreatedById = user.Id,
                UpdatedById = user.Id

            };
        }

        public static List<StakeholderMatrixEntry> ToEntities(this IEnumerable<StakeholderMatrixIncomingDto> dtos, UserOutgoingDto user)
        {
            return dtos.Select(dto => dto.ToEntity(user)).ToList();
        }
    }
}
