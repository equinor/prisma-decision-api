using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Application.Mapping;
using PrismaApi.Domain.Constants;
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
        // FacilitatorUserFilter restricts updates to facilitators only, since Role is the sole protected property.
        // If non-facilitators need to update other ProjectRole properties in the future, split the filter logic accordingly.
        if (!await _projectRoleRepository.IsUserFacilitatorFromRoleIdsAsync(dtos.Select(x => x.Id).ToList(), userDto, ct))
            throw new InvalidOperationException("Only facilitators can update project roles.");
        await _projectRoleRepository.UpdateRangeAsync(entities, FacilitatorUserFilter(userDto), ct);
        var ids = dtos.Select(d => d.Id).ToList();
        var updated = await _projectRoleRepository.GetByIdsAsync(ids, withTracking: false, filterPredicate: UserFilter(userDto), ct: ct);
        return updated.ToOutgoingDtos();
    }

    public async Task DeleteAsync(List<Guid> ids, UserOutgoingDto user, CancellationToken ct = default)
    {
        if (!await _projectRoleRepository.IsUserFacilitatorFromRoleIdsAsync(ids, user, ct))
        {
            var roles = await _projectRoleRepository.GetByIdsAsync(ids, filterPredicate: UserFilter(user), ct: ct);
            if (!roles.All(x => x.UserId == user.Id))
                throw new InvalidOperationException("Users can only delete their own project roles.");
        }

        await _projectRoleRepository.DeleteByIdsAsync(ids, filterPredicate: UserFilter(user), ct: ct);
    }

    private static Expression<Func<ProjectRole, bool>> UserFilter(UserOutgoingDto user)
        => e => e.Project!.ProjectRoles.Any(p => p.UserId == user.Id);

    private static Expression<Func<ProjectRole, bool>> FacilitatorUserFilter(UserOutgoingDto user)
        => e => e.Project!.ProjectRoles.Any(p => p.UserId == user.Id && p.Role == ProjectRoleType.Facilitator.ToString());
}
