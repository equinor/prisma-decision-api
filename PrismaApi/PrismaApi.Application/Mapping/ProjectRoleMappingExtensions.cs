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

    public static ProjectRole ToEntity(this ProjectRoleIncomingDto dto, UserOutgoingDto userDto)
    {
        return new ProjectRole
        {
            Id = dto.Id,
            UserId = dto.UserId,
            ProjectId = dto.ProjectId,
            Role = dto.Role,
            CreatedById = userDto.Id,
            UpdatedById = userDto.Id
        };
    }

    public static ProjectRole ToEntity(this ProjectRoleCreateDto dto, UserOutgoingDto userDto)
    {
        return new ProjectRole
        {
            Id = dto.Id,
            UserId = dto.UserId,
            ProjectId = dto.ProjectId,
            Role = dto.Role,
            CreatedById = userDto.Id,
            UpdatedById = userDto.Id
        };
    }

    public static List<ProjectRole> ToEntities(this IEnumerable<ProjectRoleIncomingDto> dtos, UserOutgoingDto userDto)
    {
        return dtos.Select(dto => dto.ToEntity(userDto)).ToList();
    }

    public static List<ProjectRole> ToEntities(this IEnumerable<ProjectRoleCreateDto> dtos, UserOutgoingDto userDto)
    {
        return dtos.Select(dto => dto.ToEntity(userDto)).ToList();
    }
}
