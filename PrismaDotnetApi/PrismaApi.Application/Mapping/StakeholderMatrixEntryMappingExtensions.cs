using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PrismaApi.Domain.Entities;
using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Mapping
{
    public static class StakeholderMatrixEntryMappingExtensions
    {
        public static StakeholderMatrixEntryOutgoingDto ToOutgoingDto(this StakeholderMatrixEntry entity)
        {
            return new StakeholderMatrixEntryOutgoingDto
            {
                Id = entity.Id,
                ProjectId = entity.ProjectId,
                StakeholderName = entity.StakeholderName,
                StakeholderRole = entity.StakeholderRole,
                AffectingTheDecision = entity.AffectingTheDecision,
                AffectedByTheDecision = entity.AffectedByTheDecision
            };
        }

        public static List<StakeholderMatrixEntryOutgoingDto> ToOutgoingDtos(this IEnumerable<StakeholderMatrixEntry> entities)
        {
            return entities.Select(ToOutgoingDto).ToList();
        }

        public static StakeholderMatrixEntry ToEntity(this StakeholderMatrixEntryIncomingDto dto, UserOutgoingDto user)
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

        public static List<StakeholderMatrixEntry> ToEntities(this IEnumerable<StakeholderMatrixEntryIncomingDto> dtos, UserOutgoingDto user)
        {
            return dtos.Select(dto => dto.ToEntity(user)).ToList();
        }
    }
}
