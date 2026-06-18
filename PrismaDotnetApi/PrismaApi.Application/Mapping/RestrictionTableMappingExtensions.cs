using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Mapping;

public static class RestrictionTableMappingExtensions
{
    public static RestrictionTableOutgoingDto ToOutgoingDto(this RestrictionTable entity)
    {
        return new RestrictionTableOutgoingDto
        {
            Id = entity.Id,
            ProjectId = entity.ProjectId,
            EdgeId = entity.EdgeId,
            Name = entity.Name,
            RestrictionEntries = entity.RestrictionEntries.ToOutgoingDtos(),
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
    public static List<RestrictionTableOutgoingDto> ToOutgoingDtos(this IEnumerable<RestrictionTable> entities)
    {
        return entities.Select(ToOutgoingDto).ToList();
    }
    public static RestrictionTable ToEntity(this RestrictionTableIncomingDto dto, UserOutgoingDto userDto)
    {
        return new RestrictionTable
        {
            Id = dto.Id,
            ProjectId = dto.ProjectId,
            EdgeId = dto.EdgeId,
            Name = dto.Name,
            RestrictionEntries = dto.RestrictionEntries.ToEntities(userDto),
            CreatedById = userDto.Id,
            UpdatedById = userDto.Id
        };
    }
    public static List<RestrictionTable> ToEntities(this IEnumerable<RestrictionTableIncomingDto> dtos, UserOutgoingDto userDto)
    {
        return dtos.Select(x => ToEntity(x, userDto)).ToList();
    }
}