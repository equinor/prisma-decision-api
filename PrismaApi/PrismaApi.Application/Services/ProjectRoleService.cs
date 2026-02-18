using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PrismaApi.Application.Mapping;
using PrismaApi.Application.Repositories;
using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Services;

public class ProjectRoleService
{
    private readonly ProjectRoleRepository _projectRoleRepository;

    public ProjectRoleService(ProjectRoleRepository projectRoleRepository)
    {
        _projectRoleRepository = projectRoleRepository;
    }

    public async Task<List<ProjectRoleOutgoingDto>> GetAsync(List<Guid> ids)
    {
        var roles = await _projectRoleRepository.GetByIdsAsync(ids, withTracking: false);
        return roles.ToOutgoingDtos();
    }

    public async Task<List<ProjectRoleOutgoingDto>> GetAllAsync()
    {
        var roles = await _projectRoleRepository.GetAllAsync(withTracking: false);
        return roles.ToOutgoingDtos();
    }

    public async Task<List<ProjectRoleOutgoingDto>> UpdateAsync(List<ProjectRoleIncomingDto> dtos)
    {
        var entities = dtos.ToEntities();
        await _projectRoleRepository.UpdateRangeAsync(entities);
        return entities.ToOutgoingDtos();
    }

    public async Task DeleteAsync(List<Guid> ids)
    {
        await _projectRoleRepository.DeleteByIdsAsync(ids);
    }
}
