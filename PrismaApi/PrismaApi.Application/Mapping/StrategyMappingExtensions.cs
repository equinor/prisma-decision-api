using System.Collections.Generic;
using System.Linq;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Mapping;

public static class StrategyMappingExtensions
{
    public static StrategyOutgoingDto ToOutgoingDto(this Strategy entity)
    {
        return new StrategyOutgoingDto
        {
            Id = entity.Id,
            ProjectId = entity.ProjectId,
            Name = entity.Name,
            Description = entity.Description,
            Rationale = entity.Rationale,
            Options = entity.StrategyOptions
                .Select(so => so.Option)
                .Where(option => option != null)
                .Select(option => option!.ToOutgoingDto())
                .ToList()
        };
    }

    public static List<StrategyOutgoingDto> ToOutgoingDtos(this IEnumerable<Strategy> entities)
    {
        return entities.Select(ToOutgoingDto).ToList();
    }

    public static Strategy ToEntity(this StrategyIncomingDto dto)
    {
        return new Strategy
        {
            Id = dto.Id,
            ProjectId = dto.ProjectId,
            Name = dto.Name,
            Description = dto.Description,
            Rationale = dto.Rationale,
            StrategyOptions = dto.Options.Select(option => new StrategyOption
            {
                OptionId = option.Id,
                StrategyId = dto.Id
            }).ToList()
        };
    }

    public static List<Strategy> ToEntities(this IEnumerable<StrategyIncomingDto> dtos)
    {
        return dtos.Select(ToEntity).ToList();
    }
}
