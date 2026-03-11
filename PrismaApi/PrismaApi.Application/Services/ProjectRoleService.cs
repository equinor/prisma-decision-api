using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Application.Mapping;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PrismaApi.Application.Services;

public class ProjectRoleService: IProjectRoleService
{
    private readonly IProjectRoleRepository _projectRoleRepository;

    public ProjectRoleService(IProjectRoleRepository projectRoleRepository)
    {
        _projectRoleRepository = projectRoleRepository;
    }

    public async Task<List<ProjectRoleOutgoingDto>> GetAsync(List<Guid> ids, UserOutgoingDto user)
    {
        var roles = await _projectRoleRepository.GetByIdsAsync(ids, withTracking: false, filterPredicate: UserFilter(user));
        return roles.ToOutgoingDtos();
    }

    public async Task<List<ProjectRoleOutgoingDto>> GetAllAsync(UserOutgoingDto user)
    {
        var roles = await _projectRoleRepository.GetAllAsync(withTracking: false, filterPredicate: UserFilter(user));
        return roles.ToOutgoingDtos();
    }

    public async Task<List<ProjectRoleOutgoingDto>> UpdateAsync(List<ProjectRoleIncomingDto> dtos, UserOutgoingDto userDto)
    {
        var entities = dtos.ToEntities(userDto);
        await _projectRoleRepository.UpdateRangeAsync(entities, UserFilter(userDto));
        var ids = dtos.Select(d => d.Id).ToList();
        var updated = await _projectRoleRepository.GetByIdsAsync(ids, withTracking: false, filterPredicate: UserFilter(userDto));
        return updated.ToOutgoingDtos();
    }

    public async Task DeleteAsync(List<Guid> ids, UserOutgoingDto user)
    {
        await _projectRoleRepository.DeleteByIdsAsync(ids, filterPredicate: UserFilter(user));
    }

    private static Expression<Func<ProjectRole, bool>> UserFilter(UserOutgoingDto user)
        => e => e.Project!.ProjectRoles.Any(p => p.UserId == user.Id);
}
