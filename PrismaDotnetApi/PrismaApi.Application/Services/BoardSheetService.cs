using Microsoft.Extensions.Caching.Memory;
using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Application.Mapping;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure.Caching;
using System.Linq.Expressions;

namespace PrismaApi.Application.Services;

public class BoardSheetService : IBoardSheetService
{
    private readonly IBoardSheetRepository _boardSheetRepository;
    private readonly IMemoryCache _cache;

    public BoardSheetService(IBoardSheetRepository boardSheetRepository, IMemoryCache cache)
    {
        _boardSheetRepository = boardSheetRepository;
        _cache = cache;
    }

    public async Task<List<BoardSheetOutgoingDto>> CreateAsync(List<BoardSheetIncomingDto> dtos, UserOutgoingDto userDto, CancellationToken ct = default)
    {
        var entities = dtos.ToEntities(userDto);
        await _boardSheetRepository.AddRangeAsync(entities, ct);
        return entities.ToOutgoingDtos();
    }

    public async Task<List<BoardSheetOutgoingDto>> UpdateAsync(List<BoardSheetIncomingDto> dtos, UserOutgoingDto userDto, CancellationToken ct = default)
    {
        var entities = dtos.ToEntities(userDto);
        await _boardSheetRepository.UpdateRangeAsync(entities, UserFilter(userDto), ct);
        var ids = dtos.Select(d => d.Id).ToList();
        var updated = await _boardSheetRepository.GetByIdsAsync(ids, withTracking: false, filterPredicate: UserFilter(userDto), ct: ct);
        return updated.ToOutgoingDtos();
    }

    public async Task DeleteAsync(List<Guid> ids, UserOutgoingDto user, CancellationToken ct = default)
    {
        await _boardSheetRepository.DeleteByIdsAsync(ids, filterPredicate: UserFilter(user), ct: ct);
    }

    public async Task<List<BoardSheetOutgoingDto>> GetAsync(List<Guid> ids, UserOutgoingDto user, CancellationToken ct = default)
    {
        var entities = await _boardSheetRepository.GetByIdsAsync(ids, withTracking: false, filterPredicate: UserFilter(user), ct: ct);
        return entities.ToOutgoingDtos();
    }

    public async Task<List<BoardSheetOutgoingDto>> GetAllAsync(UserOutgoingDto user, CancellationToken ct = default)
    {
        var boardSheets = new List<BoardSheetOutgoingDto>();
        var projectIdsToGetFromDb = new HashSet<Guid>();

        var projectIds = _cache.GetAccessibleProjectIds(user);

        foreach (var projectId in projectIds)
        {
            var cachedBoardSheets = _cache.GetCacheItemAsBoardSheets(projectId, user);
            if (cachedBoardSheets != null)
            {
                boardSheets.AddRange(cachedBoardSheets);
            }
            else
            {
                projectIdsToGetFromDb.Add(projectId);
            }
        }

        if (projectIdsToGetFromDb.Count > 0)
        {
            var projectBoardSheets = await _boardSheetRepository.GetAllAsync(withTracking: false, filterPredicate: ProjectFilter(projectIdsToGetFromDb), ct: ct);
            var boardSheetDtos = projectBoardSheets.ToOutgoingDtos();
            boardSheets.AddRange(boardSheetDtos);
            foreach (var projectId in projectIdsToGetFromDb)
            {
                var cacheKey = CacheKeys.GetBoardSheetsInProjectKey(projectId);
                var projectBoardSheetDtos = boardSheetDtos.Where(s => s.ProjectId == projectId).ToList();
                _cache.AddCacheItem(new CacheItem { CacheKey = cacheKey }, CacheConstants.DefaultQueryCacheInTimeSpan, projectBoardSheetDtos);
            }
        }

        return boardSheets;
    }

    private static Expression<Func<BoardSheet, bool>> UserFilter(UserOutgoingDto user)
        => e => e.Project!.ProjectRoles.Any(p => p.UserId == user.Id);

    private static Expression<Func<BoardSheet, bool>> ProjectFilter(HashSet<Guid> projectIds)
        => e => projectIds.Contains(e.ProjectId);
}
