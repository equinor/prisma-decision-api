using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Mapping;

public static class BoardNodeMappingExtensions
{
    public static BoardNodeOutgoingDto ToOutgoingDto(this BoardNode entity)
    {
        return new BoardNodeOutgoingDto
        {
            Id = entity.Id,
            ProjectId = entity.ProjectId,
            Type = entity.Type,
            Height = entity.Height,
            Width = entity.Width,
            XPosition = entity.XPosition,
            YPosition = entity.YPosition,
            Rotation = entity.Rotation,
            Data = entity.Data,
            Color = entity.Color,
            StrokeWidth = entity.StrokeWidth,
            StrokeStyle = entity.StrokeStyle,
            Opacity = entity.Opacity,
        };
    }

    public static List<BoardNodeOutgoingDto> ToOutgoingDtos(this IEnumerable<BoardNode> entities)
    {
        return entities.Select(ToOutgoingDto).ToList();
    }

    public static BoardNode ToEntity(this BoardNodeIncomingDto dto, UserOutgoingDto userDto)
    {
        return new BoardNode
        {
            Id  = dto.Id,
            ProjectId  = dto.ProjectId,
            Type = dto.Type,
            Height  = dto.Height,
            Width  = dto.Width,
            XPosition  = dto.XPosition,
            YPosition  = dto.YPosition,
            Rotation = dto.Rotation,
            Data = dto.Data,
            Color = dto.Color,
            StrokeWidth = dto.StrokeWidth,
            StrokeStyle = dto.StrokeStyle,
            Opacity = dto.Opacity,
            CreatedById = userDto.Id,
            UpdatedById = userDto.Id,
        };
    }

    public static List<BoardNode> ToEntities(this IEnumerable<BoardNodeIncomingDto> dtos, UserOutgoingDto userDto)
    {
        return dtos.Select(x => ToEntity(x, userDto)).ToList();
    }
}
