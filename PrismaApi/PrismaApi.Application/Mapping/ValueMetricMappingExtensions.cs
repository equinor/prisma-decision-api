using System.Collections.Generic;
using System.Linq;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Mapping;

public static class ValueMetricMappingExtensions
{
    public static ValueMetricOutgoingDto ToOutgoingDto(this ValueMetric entity)
    {
        return new ValueMetricOutgoingDto
        {
            Id = entity.Id,
            Name = entity.Name
        };
    }

    public static List<ValueMetricOutgoingDto> ToOutgoingDtos(this IEnumerable<ValueMetric> entities)
    {
        return entities.Select(ToOutgoingDto).ToList();
    }

    public static ValueMetric ToEntity(this ValueMetricIncomingDto dto)
    {
        return new ValueMetric
        {
            Id = dto.Id,
            Name = dto.Name
        };
    }

    public static List<ValueMetric> ToEntities(this IEnumerable<ValueMetricIncomingDto> dtos)
    {
        return dtos.Select(ToEntity).ToList();
    }
}
