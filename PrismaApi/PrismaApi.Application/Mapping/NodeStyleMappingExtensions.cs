using System.Collections.Generic;
using System.Linq;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Mapping;

public static class NodeStyleMappingExtensions
{
    public static NodeStyleOutgoingDto ToOutgoingDto(this NodeStyle entity)
    {
        return new NodeStyleOutgoingDto
        {
            Id = entity.Id,
            NodeId = entity.NodeId,
            XPosition = entity.XPosition,
            YPosition = entity.YPosition
        };
    }

    public static List<NodeStyleOutgoingDto> ToOutgoingDtos(this IEnumerable<NodeStyle> entities)
    {
        return entities.Select(ToOutgoingDto).ToList();
    }

    public static NodeStyle ToEntity(this NodeStyleIncomingDto dto)
    {
        return new NodeStyle
        {
            Id = dto.Id,
            NodeId = dto.NodeId,
            XPosition = dto.XPosition,
            YPosition = dto.YPosition
        };
    }

    public static List<NodeStyle> ToEntities(this IEnumerable<NodeStyleIncomingDto> dtos)
    {
        return dtos.Select(ToEntity).ToList();
    }
}
