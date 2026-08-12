using System.Linq.Expressions;
using Microsoft.Extensions.Caching.Memory;
using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Application.Mapping;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;
using PrismaApi.Domain.Extensions;
using PrismaApi.Infrastructure.Caching;

namespace PrismaApi.Application.Services;



public class InfluenceDiagramService : IInfluenceDiagramService
{
    private readonly IIssueRepository _issueRepository;
    private readonly IEdgeRepository _edgeRepository;
    private readonly IDiscreteProbabilityRepository _discreteProbabilityRepository;
    private readonly IDiscreteUtilityRepository _discreteUtilityRepository;
    private readonly IRestrictionTableRepository _restrictionTableRepository;
    private readonly IMemoryCache _cache;
    public InfluenceDiagramService(
        IIssueRepository issueRepository,
        IEdgeRepository edgeRepository,
        IDiscreteProbabilityRepository discreteProbabilityRepository,
        IDiscreteUtilityRepository discreteUtilityRepository,
        IRestrictionTableRepository restrictionTableRepository,
        IMemoryCache cache)
    {
        _issueRepository = issueRepository;
        _edgeRepository = edgeRepository;
        _discreteProbabilityRepository = discreteProbabilityRepository;
        _discreteUtilityRepository = discreteUtilityRepository;
        _restrictionTableRepository = restrictionTableRepository;
        _cache = cache;
    }
    public async Task<InfluenceDiagramDto> GetInfluenceDiagramAsync(Guid projectId, UserOutgoingDto user, CancellationToken ct = default)
    {
        var cachedDiagram = _cache.GetCacheItemAsInfluenceDiagram(projectId, user);
        if (cachedDiagram != null)
        {
            return cachedDiagram;
        }

        var issueEntities = await _issueRepository.GetIssuesInInfluenceDiagram(projectId, IssuesUserFilter(user), ct);
        var edgeEntities = await _edgeRepository.GetEdgesInInfluenceDiagram(projectId, EdgesUserFilter(user), ct);
        var discreteProbabilities = await _discreteProbabilityRepository.GetAllAsync(filterPredicate: e => e.Uncertainty!.Issue!.ProjectId == projectId, ct: ct);
        var discreteUtilities = await _discreteUtilityRepository.GetAllAsync(filterPredicate: e => e.Utility!.Issue!.ProjectId == projectId, ct: ct);
        var restrictionTables = await _restrictionTableRepository.GetAllAsync(filterPredicate: e => e.ProjectId == projectId, ct: ct);
        var diagram = new InfluenceDiagramDto
        {
            projectId = projectId,
            issues = issueEntities.ToOutgoingDtos(),
            edges = edgeEntities.ToOutgoingDtos(),
            discreteProbabilities = discreteProbabilities.ToDtos(),
            discreteUtilities = discreteUtilities.ToDtos(),
            restrictionTables = restrictionTables.ToOutgoingDtos(),
        };

        _cache.AddCacheItem(new CacheItem { CacheKey = CacheKeys.GetInfluenceDiagramKey(projectId) }, CacheConstants.DefaultMediumQueryCacheInTimeSpan, diagram);
        return diagram;
    }

    public async Task<InfluenceDiagramDto> GetRestrictedInfluenceDiagramAsync(Guid projectId, UserOutgoingDto user, CancellationToken ct = default)
    {
        var diagram = await GetInfluenceDiagramAsync(projectId, user, ct);
        var influenceDiagramDto = diagram.DeepClone();
        influenceDiagramDto.ApplyRestrictions();
        return influenceDiagramDto;
    }

    private static Expression<Func<Issue, bool>> IssuesUserFilter(UserOutgoingDto user)
        => e => e.Project!.Public || e.Project!.ProjectRoles.Any(p => p.UserId == user.Id);

    private static Expression<Func<Edge, bool>> EdgesUserFilter(UserOutgoingDto user)
        => e => (e.HeadNode!.Issue!.Project!.Public || e.HeadNode!.Issue!.Project!.ProjectRoles.Any(p => p.UserId == user.Id)) && (e.TailNode!.Issue!.Project!.Public || e.TailNode!.Issue!.Project!.ProjectRoles.Any(p => p.UserId == user.Id));
}
