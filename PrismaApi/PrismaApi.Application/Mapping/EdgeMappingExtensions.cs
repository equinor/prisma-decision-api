using System.Collections.Generic;
using System.Linq;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Mapping;

public static class EdgeMappingExtensions
{
    public static EdgeOutgoingDto ToOutgoingDto(this Edge entity)
    {
        return new EdgeOutgoingDto
        {
            Id = entity.Id,
            TailId = entity.TailId,
            HeadId = entity.HeadId,
            ProjectId = entity.ProjectId,
            HeadIssueId = entity.HeadNode?.IssueId ?? System.Guid.Empty,
            TailIssueId = entity.TailNode?.IssueId ?? System.Guid.Empty,
            HeadNode = entity.HeadNode != null ? entity.HeadNode.ToOutgoingDto() : new NodeOutgoingDto(),
            TailNode = entity.TailNode != null ? entity.TailNode.ToOutgoingDto() : new NodeOutgoingDto()
        };
    }

    public static List<EdgeOutgoingDto> ToOutgoingDtos(this IEnumerable<Edge> entities)
    {
        return entities.Select(ToOutgoingDto).ToList();
    }

    public static Edge ToEntity(this EdgeIncomingDto dto)
    {
        return new Edge
        {
            Id = dto.Id,
            TailId = dto.TailId,
            HeadId = dto.HeadId,
            ProjectId = dto.ProjectId
        };
    }

    public static List<Edge> ToEntities(this IEnumerable<EdgeIncomingDto> dtos)
    {
        return dtos.Select(ToEntity).ToList();
    }
}
