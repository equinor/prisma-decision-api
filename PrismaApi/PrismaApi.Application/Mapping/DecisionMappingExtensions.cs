using System.Collections.Generic;
using System.Linq;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Mapping;

public static class DecisionMappingExtensions
{
    public static DecisionOutgoingDto ToOutgoingDto(this Decision entity)
    {
        return new DecisionOutgoingDto
        {
            Id = entity.Id,
            IssueId = entity.IssueId,
            Type = entity.Type,
            Options = entity.Options.ToOutgoingDtos()
        };
    }

    public static List<DecisionOutgoingDto> ToOutgoingDtos(this IEnumerable<Decision> entities)
    {
        return entities.Select(ToOutgoingDto).ToList();
    }

    public static Decision ToEntity(this DecisionIncomingDto dto)
    {
        return new Decision
        {
            Id = dto.Id,
            IssueId = dto.IssueId,
            Type = dto.Type,
            Options = dto.Options.ToEntities()
        };
    }

    public static List<Decision> ToEntities(this IEnumerable<DecisionIncomingDto> dtos)
    {
        return dtos.Select(ToEntity).ToList();
    }
}
