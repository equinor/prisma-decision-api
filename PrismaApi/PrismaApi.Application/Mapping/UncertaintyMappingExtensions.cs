using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Mapping;

public static class UncertaintyMappingExtensions
{
    public static UncertaintyOutgoingDto ToOutgoingDto(this Uncertainty entity)
    {
        return new UncertaintyOutgoingDto
        {
            Id = entity.Id,
            IssueId = entity.IssueId,
            IsKey = entity.IsKey,
            Outcomes = entity.Outcomes.ToOutgoingDtos(),
            DiscreteProbabilities = entity.DiscreteProbabilities.ToDtos()
        };
    }

    public static List<UncertaintyOutgoingDto> ToOutgoingDtos(this IEnumerable<Uncertainty> entities)
    {
        return entities.Select(ToOutgoingDto).ToList();
    }

    public static Uncertainty ToEntity(this UncertaintyIncomingDto dto)
    {
        return new Uncertainty
        {
            Id = dto.Id,
            IssueId = dto.IssueId,
            IsKey = dto.IsKey,
            Outcomes = dto.Outcomes.ToEntities(),
            DiscreteProbabilities = dto.DiscreteProbabilities.Select(x => x.ToEntity()).ToList()
        };
    }

    public static List<Uncertainty> ToEntities(this IEnumerable<UncertaintyIncomingDto> dtos)
    {
        return dtos.Select(ToEntity).ToList();
    }
}
