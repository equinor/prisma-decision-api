using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Application.Mapping;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;
using System.Linq.Expressions;

namespace PrismaApi.Application.Services;

public class RestrictionEntryService : IRestrictionEntryService
{
    private readonly IRestrictionEntryRepository _restrictionEntryRepository;

    public RestrictionEntryService(IRestrictionEntryRepository restrictionEntryRepository)
    {
        _restrictionEntryRepository = restrictionEntryRepository;
    }

    public async Task<List<RestrictionEntryOutgoingDto>> CreateAsync(List<RestrictionEntryIncomingDto> dtos, UserOutgoingDto userDto, CancellationToken ct = default)
    {
        var entities = dtos.ToEntities(userDto);
        await _restrictionEntryRepository.AddRangeAsync(entities, ct);
        return entities.ToOutgoingDtos();
    }

    public async Task<List<RestrictionEntryOutgoingDto>> UpdateAsync(List<RestrictionEntryIncomingDto> dtos, UserOutgoingDto userDto, CancellationToken ct = default)
    {
        var entities = dtos.ToEntities(userDto);
        await _restrictionEntryRepository.UpdateRangeAsync(entities, UserFilter(userDto), ct);
        var ids = dtos.Select(d => d.Id).ToList();
        var updated = await _restrictionEntryRepository.GetByIdsAsync(ids, withTracking: false, filterPredicate: UserFilter(userDto), ct: ct);
        return updated.ToOutgoingDtos();
    }

    public async Task DeleteAsync(List<Guid> ids, UserOutgoingDto user, CancellationToken ct = default)
    {
        await _restrictionEntryRepository.DeleteByIdsAsync(ids, filterPredicate: UserFilter(user), ct: ct);
    }

    public async Task<List<RestrictionEntryOutgoingDto>> GetAsync(List<Guid> ids, UserOutgoingDto user, CancellationToken ct = default)
    {
        var entities = await _restrictionEntryRepository.GetByIdsAsync(ids, withTracking: false, filterPredicate: UserFilter(user), ct: ct);
        return entities.ToOutgoingDtos();
    }

    public async Task<List<RestrictionEntryOutgoingDto>> GetAllAsync(UserOutgoingDto user, CancellationToken ct = default)
    {
        var entities = await _restrictionEntryRepository.GetAllAsync(withTracking: false, filterPredicate: UserFilter(user), ct: ct);
        return entities.ToOutgoingDtos();
    }

    private static Expression<Func<RestrictionEntry, bool>> UserFilter(UserOutgoingDto user)
        => e => e.Project!.ProjectRoles.Any(p => p.UserId == user.Id);
}
