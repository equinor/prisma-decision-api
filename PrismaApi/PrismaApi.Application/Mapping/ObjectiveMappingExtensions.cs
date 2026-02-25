using System.Collections.Generic;
using System.Linq;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Mapping;

public static class ObjectiveMappingExtensions
{
    public static ObjectiveOutgoingDto ToOutgoingDto(this Objective entity)
    {
        return new ObjectiveOutgoingDto
        {
            Id = entity.Id,
            ProjectId = entity.ProjectId,
            Name = entity.Name,
            Type = entity.Type,
            Description = entity.Description,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    public static List<ObjectiveOutgoingDto> ToOutgoingDtos(this IEnumerable<Objective> entities)
    {
        return entities.Select(ToOutgoingDto).ToList();
    }

    public static Objective ToEntity(this ObjectiveIncomingDto dto, UserOutgoingDto userDto)
    {
        return new Objective
        {
            Id = dto.Id,
            ProjectId = dto.ProjectId,
            Name = dto.Name,
            Type = dto.Type,
            Description = dto.Description,
            CreatedById = userDto.Id,
            UpdatedById = userDto.Id,
        };
    }

    public static Objective ToEntity(this ObjectiveViaProjectDto dto, Guid projectId, UserOutgoingDto userDto)
    {
        return new Objective
        {
            Id = dto.Id,
            ProjectId = projectId,
            Name = dto.Name,
            Type = dto.Type,
            Description = dto.Description,
            CreatedById = userDto.Id,
            UpdatedById = userDto.Id,
        };
    }

    public static List<Objective> ToEntities(this IEnumerable<ObjectiveIncomingDto> dtos, UserOutgoingDto userDto)
    {
        return dtos.Select(x => ToEntity(x, userDto)).ToList();
    }

    public static List<Objective> ToEntities(this IEnumerable<ObjectiveViaProjectDto> dtos, Guid projectId, UserOutgoingDto userDto)
    {
        return dtos.Select(dto => dto.ToEntity(projectId, userDto)).ToList();
    }
}
