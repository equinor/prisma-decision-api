using System.Collections.Generic;
using System.Linq;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Mapping;

public static class ProjectMappingExtensions
{
    public static ProjectOutgoingDto ToOutgoingDto(this Project entity)
    {
        return new ProjectOutgoingDto
        {
            Id = entity.Id,
            Name = entity.Name,
            ParentProjectId = entity.ParentProjectId,
            ParentProjectName = entity.ParentProjectName ?? "",
            OpportunityStatement = entity.OpportunityStatement,
            Public = entity.Public,
            EndDate = entity.EndDate,
            Objectives = entity.Objectives.ToOutgoingDtos(),
            Strategies = entity.Strategies.ToOutgoingDtos(),
            Users = entity.ProjectRoles.ToOutgoingDtos()
        };
    }

    public static PopulatedProjectDto ToPopulatedDto(this Project entity)
    {
        return new PopulatedProjectDto
        {
            Id = entity.Id,
            Name = entity.Name,
            ParentProjectId = entity.ParentProjectId,
            ParentProjectName = entity.ParentProjectName ?? "",
            OpportunityStatement = entity.OpportunityStatement,
            Public = entity.Public,
            EndDate = entity.EndDate,
            Objectives = entity.Objectives.ToOutgoingDtos(),
            Strategies = entity.Strategies.ToOutgoingDtos(),
            Users = entity.ProjectRoles.ToOutgoingDtos()
        };
    }

    public static List<ProjectOutgoingDto> ToOutgoingDtos(this IEnumerable<Project> entities)
    {
        return entities.Select(ToOutgoingDto).ToList();
    }

    public static List<PopulatedProjectDto> ToPopulatedDtos(this IEnumerable<Project> entities)
    {
        return entities.Select(ToPopulatedDto).ToList();
    }

    public static Project ToEntity(this ProjectCreateDto dto, UserOutgoingDto userDto)
    {
        return new Project
        {
            Id = dto.Id,
            ParentProjectId = dto.ParentProjectId,
            ParentProjectName = dto.ParentProjectName,
            Name = dto.Name,
            OpportunityStatement = dto.OpportunityStatement,
            Public = dto.Public,
            EndDate = dto.EndDate,
            CreatedById = userDto.Id,
            UpdatedById = userDto.Id,
            Objectives = dto.Objectives.ToEntities(dto.Id, userDto)
        };
    }

    public static Project ToEntity(this ProjectIncomingDto dto, UserOutgoingDto userDto)
    {
        return new Project
        {
            Id = dto.Id,
            ParentProjectId = dto.ParentProjectId,
            ParentProjectName = dto.ParentProjectName,
            Name = dto.Name,
            OpportunityStatement = dto.OpportunityStatement,
            Public = dto.Public,
            EndDate = dto.EndDate,
            CreatedById = userDto.Id,
            UpdatedById = userDto.Id,
            Objectives = dto.Objectives.ToEntities(dto.Id, userDto),
            Strategies = dto.Strategies.ToEntities(userDto),
            ProjectRoles = dto.Users.ToEntities(userDto)
        };
    }

    public static List<Project> ToEntities(this IEnumerable<ProjectCreateDto> dtos, UserOutgoingDto userDto)
    {
        return dtos.Select(dto => dto.ToEntity(userDto)).ToList();
    }

    public static List<Project> ToEntities(this IEnumerable<ProjectIncomingDto> dtos, UserOutgoingDto userDto)
    {
        return dtos.Select(dto => dto.ToEntity(userDto)).ToList();
    }
}
