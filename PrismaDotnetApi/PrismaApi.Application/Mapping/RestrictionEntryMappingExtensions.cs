using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Mapping;

public static class RestrictionEntryMappingExtensions
{
    public static RestrictionEntryOutgoingDto ToOutgoingDto(this RestrictionEntry entity)
    {

        Guid? parentStateId = entity.ParentOptionId.HasValue ? entity.ParentOptionId : entity.ParentOutcomeId;
        Guid? childStateId = entity.ChildOptionId.HasValue ? entity.ChildOptionId : entity.ChildOutcomeId;
        bool isParentUncertainty = entity.ParentOutcomeId.HasValue;
        bool isChildUncertainty = entity.ChildOutcomeId.HasValue;
        return new RestrictionEntryOutgoingDto
        {
            Id = entity.Id,
            ProjectId = entity.ProjectId,
            RestrictionTableId = entity.RestrictionTableId,
            Name = entity.Name,
            RestrictionValue = entity.RestrictionValue,
            ParentStateId = parentStateId,
            IsParentUncertainty = isParentUncertainty,
            ChildStateId = childStateId,
            IsChildUncertainty = isChildUncertainty,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    public static List<RestrictionEntryOutgoingDto> ToOutgoingDtos(this IEnumerable<RestrictionEntry> entities)
    {
        return entities.Select(ToOutgoingDto).ToList();
    }

    public static RestrictionEntry ToEntity(this RestrictionEntryIncomingDto dto, UserOutgoingDto userDto)
    {
        Guid? parentOptionId = dto.IsParentUncertainty ? null : dto.ParentStateId;
        Guid? parentOutcomeId = dto.IsParentUncertainty ? dto.ParentStateId : null;
        Guid? childOptionId = dto.IsChildUncertainty ? null : dto.ChildStateId;
        Guid? childOutcomeId = dto.IsChildUncertainty ? dto.ChildStateId : null;
        return new RestrictionEntry
        {
            Id = dto.Id,
            ProjectId = dto.ProjectId,
            RestrictionTableId = dto.RestrictionTableId,
            Name = dto.Name,
            RestrictionValue = dto.RestrictionValue,
            ParentOptionId = parentOptionId,
            ParentOutcomeId = parentOutcomeId,
            ChildOptionId = childOptionId,
            ChildOutcomeId = childOutcomeId,
            CreatedById = userDto.Id,
            UpdatedById = userDto.Id
        };
    }
    public static List<RestrictionEntry> ToEntities(this IEnumerable<RestrictionEntryIncomingDto> dtos, UserOutgoingDto userDto)
    {
        return dtos.Select(x => ToEntity(x, userDto)).ToList();
    }
}