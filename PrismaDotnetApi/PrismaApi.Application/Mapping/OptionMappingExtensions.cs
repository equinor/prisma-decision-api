using System;
using System.Collections.Generic;
using System.Linq;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Mapping;

public static class OptionMappingExtensions
{
    public static OptionOutgoingDto ToOutgoingDto(this Option entity)
    {
        return new OptionOutgoingDto
        {
            Id = entity.Id,
            Name = entity.Name,
            DecisionId = entity.DecisionId,
            ProjectId = entity.ProjectId,
            Utility = entity.Utility,
            CreatedAt = entity.CreatedAt

        };
    }

    public static List<OptionOutgoingDto> ToOutgoingDtos(this IEnumerable<Option> entities)
    {
        return entities.Select(ToOutgoingDto).ToList();
    }

    public static Option ToEntity(this OptionIncomingDto dto)
    {
        return new Option
        {
            Id = dto.Id,
            Name = dto.Name,
            DecisionId = dto.DecisionId,
            ProjectId = dto.ProjectId,
            Utility = dto.Utility,
            CreatedAt = dto.CreatedAt
        };
    }

    public static List<Option> ToEntities(this IEnumerable<OptionIncomingDto> dtos)
    {
        return dtos.Select(ToEntity).ToList();
    }
}
