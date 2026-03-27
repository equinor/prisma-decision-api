using System.Collections.Generic;
using System.Linq;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Mapping;

public static class UtilityMappingExtensions
{
    public static UtilityOutgoingDto ToOutgoingDto(this Utility entity)
    {
        return new UtilityOutgoingDto
        {
            Id = entity.Id,
            IssueId = entity.IssueId,
            DiscreteUtilities = entity.DiscreteUtilities.ToDtos()
        };
    }

    public static List<UtilityOutgoingDto> ToOutgoingDtos(this IEnumerable<Utility> entities)
    {
        return entities.Select(ToOutgoingDto).ToList();
    }

    public static Utility ToEntity(this UtilityIncomingDto dto)
    {
        return new Utility
        {
            Id = dto.Id,
            IssueId = dto.IssueId
        };
    }

    public static List<Utility> ToEntities(this IEnumerable<UtilityIncomingDto> dtos)
    {
        return dtos.Select(ToEntity).ToList();
    }
}
