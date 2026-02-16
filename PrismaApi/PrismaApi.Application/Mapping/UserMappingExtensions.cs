using System.Collections.Generic;
using System.Linq;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Mapping;

public static class UserMappingExtensions
{
    public static UserOutgoingDto ToOutgoingDto(this User entity)
    {
        return new UserOutgoingDto
        {
            Id = entity.Id,
            Name = entity.Name,
            AzureId = entity.AzureId,
            ProjectRoles = entity.ProjectRoles.ToOutgoingDtos()
        };
    }

    public static List<UserOutgoingDto> ToOutgoingDtos(this IEnumerable<User> entities)
    {
        return entities.Select(ToOutgoingDto).ToList();
    }

    public static User ToEntity(this UserIncomingDto dto)
    {
        return new User
        {
            Id = dto.Id ?? System.Guid.NewGuid(),
            Name = dto.Name,
            AzureId = dto.AzureId,
            ProjectRoles = dto.ProjectRoles.ToEntities()
        };
    }
}
