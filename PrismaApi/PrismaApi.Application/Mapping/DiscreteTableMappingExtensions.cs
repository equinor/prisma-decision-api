using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Mapping;

public static class DiscreteTableMappingExtensions
{
    public static DiscreteProbabilityDto ToDto(this DiscreteProbability entity)
    {
        return new DiscreteProbabilityDto
        {
            Id = entity.Id,
            OutcomeId = entity.OutcomeId,
            UncertaintyId = entity.UncertaintyId,
            Probability = entity.Probability,
            ParentOptionIds = entity.ParentOptions.Select(x => x.ParentOptionId).ToList(),
            ParentOutcomeIds = entity.ParentOutcomes.Select(x => x.ParentOutcomeId).ToList(),
            //ParentOutcomes = entity.ParentOutcomes.Select(ToDto).ToList(),
            //ParentOptions = entity.ParentOptions.Select(ToDto).ToList()
        };
    }

    public static DiscreteProbability ToEntity(this DiscreteProbabilityDto dto)
    {
        return new DiscreteProbability
        {
            Id = dto.Id,
            OutcomeId = dto.OutcomeId,
            UncertaintyId = dto.UncertaintyId,
            Probability = dto.Probability,
            ParentOptions = dto.ParentOptionIds
                .Select(x => new DiscreteProbabilityParentOption
                {
                    DiscreteProbabilityId = dto.Id,
                    ParentOptionId = x
                }).ToList(),
            ParentOutcomes = dto.ParentOutcomeIds
                .Select(x => new DiscreteProbabilityParentOutcome
                {
                    DiscreteProbabilityId = dto.Id,
                    ParentOutcomeId = x
                }).ToList()
        };
    }

    public static List<DiscreteProbabilityDto> ToDtos(this IEnumerable<DiscreteProbability> entities)
    {
        return entities.Select(ToDto).ToList();
    }

    public static DiscreteProbabilityParentOutcomeDto ToDto(this DiscreteProbabilityParentOutcome entity)
    {
        return new DiscreteProbabilityParentOutcomeDto
        {
            DiscreteProbabilityId = entity.DiscreteProbabilityId,
            ParentOutcomeId = entity.ParentOutcomeId
        };
    }

    public static DiscreteProbabilityParentOptionDto ToDto(this DiscreteProbabilityParentOption entity)
    {
        return new DiscreteProbabilityParentOptionDto
        {
            DiscreteProbabilityId = entity.DiscreteProbabilityId,
            ParentOptionId = entity.ParentOptionId
        };
    }

    public static DiscreteUtilityDto ToDto(this DiscreteUtility entity)
    {
        return new DiscreteUtilityDto
        {
            Id = entity.Id,
            UtilityId = entity.UtilityId,
            ValueMetricId = entity.ValueMetricId,
            UtilityValue = entity.UtilityValue,
            ParentOptionIds = entity.ParentOptions.Select(x => x.ParentOptionId).ToList(),
            ParentOutcomeIds = entity.ParentOutcomes.Select(x => x.ParentOutcomeId).ToList(),
        };
    }

    public static List<DiscreteUtilityDto> ToDtos(this IEnumerable<DiscreteUtility> entities)
    {
        return entities.Select(ToDto).ToList();
    }

    public static DiscreteUtilityParentOutcomeDto ToDto(this DiscreteUtilityParentOutcome entity)
    {
        return new DiscreteUtilityParentOutcomeDto
        {
            DiscreteUtilityId = entity.DiscreteUtilityId,
            ParentOutcomeId = entity.ParentOutcomeId
        };
    }

    public static DiscreteUtilityParentOptionDto ToDto(this DiscreteUtilityParentOption entity)
    {
        return new DiscreteUtilityParentOptionDto
        {
            DiscreteUtilityId = entity.DiscreteUtilityId,
            ParentOptionId = entity.ParentOptionId
        };
    }
}
