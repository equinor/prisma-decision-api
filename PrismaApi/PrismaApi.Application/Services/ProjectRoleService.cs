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

    public async Task<List<ProjectRoleOutgoingDto>> GetAsync(List<Guid> ids, UserOutgoingDto user, CancellationToken ct = default)
    {
        var roles = await _projectRoleRepository.GetByIdsAsync(ids, withTracking: false, filterPredicate: UserFilter(user), ct: ct);
        return roles.ToOutgoingDtos();
    }

    public async Task<List<ProjectRoleOutgoingDto>> GetAllAsync(UserOutgoingDto user, CancellationToken ct = default)
    {
        var roles = await _projectRoleRepository.GetAllAsync(withTracking: false, filterPredicate: UserFilter(user), ct: ct);
        return roles.ToOutgoingDtos();
    }

    public async Task<List<ProjectRoleOutgoingDto>> UpdateAsync(List<ProjectRoleIncomingDto> dtos, UserOutgoingDto userDto, CancellationToken ct = default)
    {
        var entities = dtos.ToEntities(userDto);
        await _projectRoleRepository.UpdateRangeAsync(entities, UserFilter(userDto), ct);
        var ids = dtos.Select(d => d.Id).ToList();
        var updated = await _projectRoleRepository.GetByIdsAsync(ids, withTracking: false, filterPredicate: UserFilter(userDto), ct: ct);
        return updated.ToOutgoingDtos();
    }

    public async Task DeleteAsync(List<Guid> ids, UserOutgoingDto user, CancellationToken ct = default)
    {
        // var rolesToDelete = await _projectRoleRepository.GetByIdsAsync(ids, withTracking: false, filterPredicate: UserFilter(user), ct: ct);

        // if (rolesToDelete.Count == 0)
        // {
        //     return;
        // }

        // var affectedProjectIds = rolesToDelete.Select(r => r.ProjectId).Distinct().ToList();
        // var allRolesInAffectedProjects = await _projectRoleRepository.GetAllAsync(
        //     withTracking: false,
        //     filterPredicate: r => affectedProjectIds.Contains(r.ProjectId),
        //     ct: ct);

        // // affectedProjectIds should in most cases only have a length of 1
        // foreach (var projectId in affectedProjectIds)
        // {
        //     var totalRoles = allRolesInAffectedProjects.Count(r => r.ProjectId == projectId);
        //     var deletingCount = rolesToDelete.Count(r => r.ProjectId == projectId);
        //     if (totalRoles - deletingCount == 0)
        //     {
        //         throw new ArgumentException("Project(s) must have at least one project role.");
        //     }
        // }

        await _projectRoleRepository.DeleteByIdsAsync(ids, filterPredicate: UserFilter(user), ct: ct);
    }

    private static Expression<Func<ProjectRole, bool>> UserFilter(UserOutgoingDto user)
        => e => e.Project!.ProjectRoles.Any(p => p.UserId == user.Id);
}
