using System.Collections.Generic;
using System.Linq;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Mapping;

public static class OutcomeMappingExtensions
{
    public static OutcomeOutgoingDto ToOutgoingDto(this Outcome entity)
    {
        return new OutcomeOutgoingDto
        {
            Id = entity.Id,
            Name = entity.Name,
            UncertaintyId = entity.UncertaintyId,
            Utility = entity.Utility
        };
    }

    public static List<OutcomeOutgoingDto> ToOutgoingDtos(this IEnumerable<Outcome> entities)
    {
        return entities.Select(ToOutgoingDto).ToList();
    }

    public static Outcome ToEntity(this OutcomeIncomingDto dto)
    {
        return new Outcome
        {
            Id = dto.Id,
            Name = dto.Name,
            UncertaintyId = dto.UncertaintyId,
            Utility = dto.Utility
        };
    }

    public static List<Outcome> ToEntities(this IEnumerable<OutcomeIncomingDto> dtos)
    {
        return dtos.Select(ToEntity).ToList();
    }
}
