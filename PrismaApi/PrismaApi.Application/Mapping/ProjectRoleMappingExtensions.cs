using System.Collections.Generic;
using System.Linq;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Mapping;

public static class ProjectRoleMappingExtensions
{
    public static ProjectRoleOutgoingDto ToOutgoingDto(this ProjectRole entity)
    {
        return new ProjectRoleOutgoingDto
        {
            Id = entity.Id,
            UserId = entity.UserId,
            ProjectId = entity.ProjectId,
            Role = entity.Role,
            UserName = entity.User?.Name ?? string.Empty,
            AzureId = entity.User?.AzureId ?? string.Empty
        };
    }

    public static List<ProjectRoleOutgoingDto> ToOutgoingDtos(this IEnumerable<ProjectRole> entities)
    {
        return entities.Select(ToOutgoingDto).ToList();
    }

    public static ProjectRole ToEntity(this ProjectRoleIncomingDto dto)
    {
        return new ProjectRole
        {
            Id = dto.Id,
            UserId = dto.UserId,
            ProjectId = dto.ProjectId,
            Role = dto.Role
        };
    }

    public static ProjectRole ToEntity(this ProjectRoleCreateDto dto)
    {
        return new ProjectRole
        {
            Id = dto.Id,
            UserId = dto.UserId,
            ProjectId = dto.ProjectId,
            Role = dto.Role
        };
    }

    public static List<ProjectRole> ToEntities(this IEnumerable<ProjectRoleIncomingDto> dtos)
    {
        return dtos.Select(ToEntity).ToList();
    }

    public static List<ProjectRole> ToEntities(this IEnumerable<ProjectRoleCreateDto> dtos)
    {
        return dtos.Select(ToEntity).ToList();
    }
}
