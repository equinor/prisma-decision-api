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
        var user = new User
        {
            Name = dto.Name,
            AzureId = dto.AzureId,
            ProjectRoles = dto.ProjectRoles.ToEntities()
        };

        if (dto?.Id.HasValue == true)
        {
            user.Id = dto.Id.Value;
        }

        return user;
    }
}
