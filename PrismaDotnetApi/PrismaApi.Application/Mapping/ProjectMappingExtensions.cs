using PrismaApi.Domain.Constants;
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
            Users = entity.ProjectRoles.ToOutgoingDtos(),
            BoardNodes = entity.BoardNodes.ToOutgoingDtos(),
        };
    }

    public static PopulatedProjectDto ToPopulatedDto(this Project entity)
    {
        return new PopulatedProjectDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Objectives = entity.Objectives.ToOutgoingDtos(),
            Strategies = entity.Strategies.ToOutgoingDtos(),
            ParentProjectId = entity.ParentProjectId,
            ParentProjectName = entity.ParentProjectName ?? "",
            OpportunityStatement = entity.OpportunityStatement,
            Public = entity.Public,
            EndDate = entity.EndDate,
            Users = entity.ProjectRoles.ToOutgoingDtos(),
            BoardNodes = entity.BoardNodes.ToOutgoingDtos(),

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

    public static FullProjectForDuplicationDto ToFullProjectForDuplicationDto(this Project entity)
    {
        return new FullProjectForDuplicationDto
        {
            Id = entity.Id,
            Name = entity.Name,
            ParentProjectId = entity.ParentProjectId,
            ParentProjectName = entity.ParentProjectName ?? "",
            OpportunityStatement = entity.OpportunityStatement,
            Public = entity.Public,
            EndDate = entity.EndDate,
            Users = entity.ProjectRoles.ToOutgoingDtos(),
            strategies = entity.Strategies.ToOutgoingDtos(),
            objectives = entity.Objectives.ToOutgoingDtos(),
            Issues = entity.Issues.ToOutgoingDtos(),
            Edges = entity.Edges.ToOutgoingDtos(),
            BoardNodes = entity.BoardNodes.ToOutgoingDtos(),
            Assessments = entity.Assessments.ToOutgoingDtos(),
        };
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
            ProjectRoles = dto.Users.ToEntities(userDto),
            BoardNodes = dto.BoardNodes.ToEntities(userDto),
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
            ProjectRoles = dto.Users.ToEntities(userDto),
            BoardNodes = dto.BoardNodes.ToEntities(userDto),
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
