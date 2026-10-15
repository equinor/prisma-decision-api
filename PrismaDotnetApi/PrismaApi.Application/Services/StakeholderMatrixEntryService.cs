using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Application.Mapping;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure.Caching;


namespace PrismaApi.Application.Services
{
    public class StakeholderMatrixEntryService : IStakeholderMatrixEntryService
    {
        private readonly IStakeholderMatrixEntryRepository _stakeholderMatrixRepository;
        private readonly IMemoryCache _cache;

        public StakeholderMatrixEntryService(IStakeholderMatrixEntryRepository stakeholderMatrixRepository, IMemoryCache cache)
        {
            _stakeholderMatrixRepository = stakeholderMatrixRepository;
            _cache = cache;
        }

        public async Task<List<StakeholderMatrixDto>> CreateAsync(List<StakeholderMatrixIncomingDto> dtos, UserOutgoingDto user, CancellationToken ct = default)
        {
            var entities = dtos.ToEntities(user);
            await _stakeholderMatrixRepository.AddRangeAsync(entities, ct);
            var ids = dtos.Select(d => d.Id).ToList();
            var created = await _stakeholderMatrixRepository.GetByIdsAsync(ids, withTracking: false, ct: ct);
            return created.ToOutgoingDtos();
        }

        public async Task<List<StakeholderMatrixDto>> UpdateAsync(List<StakeholderMatrixIncomingDto> dtos, UserOutgoingDto userDto, CancellationToken ct = default)
        {
            var entities = dtos.ToEntities(userDto);
            await _stakeholderMatrixRepository.UpdateRangeAsync(entities, UserFilter(userDto), ct);
            var ids = dtos.Select(d => d.Id).ToList();
            var updated = await _stakeholderMatrixRepository.GetByIdsAsync(ids, withTracking: false, filterPredicate: UserFilter(userDto), ct: ct);
            return updated.ToOutgoingDtos();
        }

        public async Task DeleteAsync(List<Guid> ids, UserOutgoingDto user, CancellationToken ct = default)
        {
            await _stakeholderMatrixRepository.DeleteByIdsAsync(ids, filterPredicate: UserFilter(user), ct: ct);
        }

        public async Task<List<StakeholderMatrixDto>> GetAsync(List<Guid> ids, UserOutgoingDto user, CancellationToken ct = default)
        {
            var entities = await _stakeholderMatrixRepository.GetByIdsAsync(ids, withTracking: false, filterPredicate: UserFilter(user), ct: ct);
            return entities.ToOutgoingDtos();
        }

        public async Task<List<StakeholderMatrixDto>> GetByProjectAsync(Guid projectId, UserOutgoingDto user, CancellationToken ct = default)
        {
            var objectives = await _stakeholderMatrixRepository.GetAllAsync(filterPredicate: ProjectAndUserFilter(projectId, user), ct: ct);
            return objectives.ToOutgoingDtos();
        }

        public async Task<List<StakeholderMatrixDto>> GetAllAsync(UserOutgoingDto user, CancellationToken ct = default)
        {
            return await _cache.GetProjectScopedAsync(
           user,
           loadMissingAsync: async (projectIds, ct) => (
               await _stakeholderMatrixRepository.GetAllAsync(
                   withTracking: false,
                   filterPredicate: ProjectFilter(projectIds),
                   ct: ct
               )
           ).ToOutgoingDtos(),
           getProjectId: dto => dto.ProjectId,
           getCacheKey: CacheKeys.GetStakeHolderMatrixEntriesInProjectKey,
           cacheDuration: CacheConstants.DefaultMediumQueryCacheInTimeSpan,
           ct: ct);
        }
        private static Expression<Func<StakeholderMatrixEntry, bool>> UserFilter(UserOutgoingDto user)
            => e => e.Project!.ProjectRoles.Any(p => p.UserId == user.Id);

        private static Expression<Func<StakeholderMatrixEntry, bool>> ProjectAndUserFilter(Guid projectId, UserOutgoingDto user)
        => e => e.Project != null && e.Project.Id == projectId && e.Project.ProjectRoles.Any(p => p.UserId == user.Id);
        private static Expression<Func<StakeholderMatrixEntry, bool>> ProjectFilter(HashSet<Guid> projectIds)
            => e => e.Project != null && projectIds.Contains(e.Project.Id);


    }


}
