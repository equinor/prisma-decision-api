using System.Collections.Generic;
using System.Linq;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Mapping;

public static class NodeMappingExtensions
{
    public static NodeOutgoingDto ToOutgoingDto(this Node entity)
    {
        return new NodeOutgoingDto
        {
            Id = entity.Id,
            IssueId = entity.IssueId,
            ProjectId = entity.ProjectId,
            Name = entity.Name,
            Issue = entity.Issue != null ? entity.Issue.ToOutgoingDtoViaNode() : new IssueViaNodeOutgoingDto(),
            NodeStyle = entity.NodeStyle != null ? entity.NodeStyle.ToOutgoingDto() : new NodeStyleOutgoingDto()
        };
    }

    public static NodeViaIssueOutgoingDto ToOutgoingDtoViaIssue(this Node entity)
    {
        return new NodeViaIssueOutgoingDto
        {
            Id = entity.Id,
            IssueId = entity.IssueId,
            ProjectId = entity.ProjectId,
            Name = entity.Name,
            NodeStyle = entity.NodeStyle != null ? entity.NodeStyle.ToOutgoingDto() : new NodeStyleOutgoingDto()
        };
    }

    public static List<NodeOutgoingDto> ToOutgoingDtos(this IEnumerable<Node> entities)
    {
        return entities.Select(ToOutgoingDto).ToList();
    }

    public static Node ToEntity(this NodeIncomingDto dto)
    {
        return new Node
        {
            Id = dto.Id,
            IssueId = dto.IssueId,
            ProjectId = dto.ProjectId,
            Name = dto.Name,
            NodeStyle = dto.NodeStyle != null ? dto.NodeStyle.ToEntity() : null
        };
    }

    public static List<Node> ToEntities(this IEnumerable<NodeIncomingDto> dtos)
    {
        return dtos.Select(ToEntity).ToList();
    }
}
