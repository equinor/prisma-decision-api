using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Mapping;

public static class BoardSheetMappingExtensions
{
    public static BoardSheetOutgoingDto ToOutgoingDto(this BoardSheet entity)
    {
        return new BoardSheetOutgoingDto
        {
            Id = entity.Id,
            ProjectId = entity.ProjectId,
            Name = entity.Name,
        };
    }

    public static List<BoardSheetOutgoingDto> ToOutgoingDtos(this IEnumerable<BoardSheet> entities)
    {
        return entities.Select(ToOutgoingDto).ToList();
    }

    public static BoardSheet ToEntity(this BoardSheetIncomingDto dto, UserOutgoingDto userDto)
    {
        return new BoardSheet
        {
            Id = dto.Id,
            ProjectId = dto.ProjectId,
            Name = dto.Name,
            CreatedById = userDto.Id,
            UpdatedById = userDto.Id,
        };
    }

    public static List<BoardSheet> ToEntities(this IEnumerable<BoardSheetIncomingDto> dtos, UserOutgoingDto userDto)
    {
        return dtos.Select(x => ToEntity(x, userDto)).ToList();
    }
}
