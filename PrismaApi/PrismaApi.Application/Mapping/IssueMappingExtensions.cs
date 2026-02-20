using System.Collections.Generic;
using System.Linq;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Mapping;

public static class IssueMappingExtensions
{
    public static IssueOutgoingDto ToOutgoingDto(this Issue entity)
    {
        return new IssueOutgoingDto
        {
            Id = entity.Id,
            ProjectId = entity.ProjectId,
            Type = entity.Type,
            Boundary = entity.Boundary,
            Name = entity.Name,
            Description = entity.Description,
            Order = entity.Order,
            Node = entity.Node != null ? entity.Node.ToOutgoingDtoViaIssue() : new NodeViaIssueOutgoingDto(),
            Decision = entity.Decision != null ? entity.Decision.ToOutgoingDto() : null,
            Uncertainty = entity.Uncertainty != null ? entity.Uncertainty.ToOutgoingDto() : null,
            Utility = entity.Utility != null ? entity.Utility.ToOutgoingDto() : null,
            CreatedAt = entity.CreatedAt ?? DateTimeOffset.UtcNow, //.ToString("O").Replace("+00:00", "Z"),
            UpdatedAt = entity.UpdatedAt ?? DateTimeOffset.UtcNow
        };
    }

    public static IssueViaNodeOutgoingDto ToOutgoingDtoViaNode(this Issue entity)
    {
        return new IssueViaNodeOutgoingDto
        {
            Id = entity.Id,
            ProjectId = entity.ProjectId,
            Type = entity.Type,
            Boundary = entity.Boundary,
            Name = entity.Name,
            Description = entity.Description,
            Order = entity.Order,
            Decision = entity.Decision != null ? entity.Decision.ToOutgoingDto() : null,
            Uncertainty = entity.Uncertainty != null ? entity.Uncertainty.ToOutgoingDto() : null,
            Utility = entity.Utility != null ? entity.Utility.ToOutgoingDto() : null
        };
    }

    public static List<IssueOutgoingDto> ToOutgoingDtos(this IEnumerable<Issue> entities)
    {
        return entities.Select(ToOutgoingDto).ToList();
    }

    public static Issue ToEntity(this IssueIncomingDto dto, UserOutgoingDto userDto)
    {
        return new Issue
        {
            Id = dto.Id,
            ProjectId = dto.ProjectId,
            Type = dto.Type,
            Boundary = dto.Boundary,
            Name = dto.Name,
            Description = dto.Description,
            Order = dto.Order,
            CreatedById = userDto.Id,
            UpdatedById = userDto.Id,
            Node = dto.Node != null ? dto.Node.ToEntity() : null,
            Decision = dto.Decision != null ? dto.Decision.ToEntity() : null,
            Uncertainty = dto.Uncertainty != null ? dto.Uncertainty.ToEntity() : null,
            Utility = dto.Utility != null ? dto.Utility.ToEntity() : null
        };
    }

    public static List<Issue> ToEntities(this IEnumerable<IssueIncomingDto> dtos, UserOutgoingDto userDto)
    {
        return dtos.Select(dto => dto.ToEntity(userDto)).ToList();
    }
}
