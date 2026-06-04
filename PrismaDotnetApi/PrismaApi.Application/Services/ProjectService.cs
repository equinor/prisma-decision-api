using Microsoft.Extensions.Caching.Memory;
using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Application.Mapping;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure.Caching;
using System.Linq.Expressions;

namespace PrismaApi.Application.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectRoleRepository _projectRoleRepository;
    private readonly IIssueRepository _issueRepository;
    private readonly IEdgeRepository _edgeRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMemoryCache _cache;

    public ProjectService(
        IProjectRepository projectRepository,
        IProjectRoleRepository projectRoleRepository,
        IIssueRepository issueRepository,
        IEdgeRepository edgeRepository,
        IUserRepository userRepository,
        IMemoryCache cache)
    {
        _projectRepository = projectRepository;
        _projectRoleRepository = projectRoleRepository;
        _issueRepository = issueRepository;
        _edgeRepository = edgeRepository;
        _userRepository = userRepository;
        _cache = cache;
        _userRepository = userRepository;
    }

    public async Task<List<ProjectOutgoingDto>> CreateAsync(List<ProjectCreateDto> dtos, bool createDefaultRole, UserOutgoingDto userDto, CancellationToken ct = default)
    {
        var projectEntities = dtos.ToEntities(userDto);

        await _projectRepository.AddRangeAsync(projectEntities, ct);

        if (createDefaultRole)
        {

            var facilitatorRole = dtos.Select(x =>
            {
                return new ProjectRoleCreateDto
                {
                    Id = Guid.NewGuid(),
                    ProjectId = x.Id,
                    UserId = userDto.Id,
                    Role = ProjectRoleType.Facilitator.ToString()
                };
            }).Distinct();

            await _projectRoleRepository.AddRangeAsync(facilitatorRole.ToEntities(userDto), ct);
        }

        var ids = projectEntities.Select(p => p.Id).ToList();
        var projects = await _projectRepository.GetByIdsAsync(ids, withTracking: false, ct: ct);
        return projects.ToOutgoingDtos();
    }

    public async Task<List<ProjectOutgoingDto>> UpdateAsync(List<ProjectIncomingDto> dtos, UserOutgoingDto userDto, CancellationToken ct = default)
    {
        var userIds = dtos.SelectMany(d => d.Users).Select(u => new { u.UserId, u.Name }).Distinct();
        foreach (var u in userIds)
        {
            await _userRepository.GetOrAddByIdAsync(new UserIncomingDto { Id = u.UserId, Name = u.Name }, ct);
        }

        var projectEntities = dtos.ToEntities(userDto);
        var projects = await _projectRepository.UpdateRangeAsync(projectEntities, userDto, filterPredicate: UserFilter(userDto), ct);
        return projects.ToOutgoingDtos();
    }

    public async Task DeleteAsync(List<Guid> ids, UserOutgoingDto user, CancellationToken ct = default)
    {
        await _projectRepository.DeleteByIdsAsync(ids, filterPredicate: UserFilter(user), ct: ct);
    }

    public async Task<List<ProjectOutgoingDto>> GetAsync(List<Guid> ids, UserOutgoingDto user, CancellationToken ct = default)
    {
        var projects = await _projectRepository.GetByIdsAsync(ids, withTracking: false, filterPredicate: UserFilter(user), ct: ct);
        var dtos = projects.ToOutgoingDtos();
        RegisterPublicProjectsInCache(dtos);
        return dtos;
    }

    public async Task<List<ProjectOutgoingDto>> GetAllAsync(UserOutgoingDto user, CancellationToken ct = default)
    {
        var projects = await _projectRepository.GetAllAsync(withTracking: false, filterPredicate: UserFilter(user), ct: ct);
        var dtos = projects.ToOutgoingDtos();
        RegisterPublicProjectsInCache(dtos);
        return dtos;
    }

    public async Task<List<PopulatedProjectDto>> GetPopulatedAsync(List<Guid> ids, UserOutgoingDto user, CancellationToken ct = default)
    {
        var projects = await _projectRepository.GetByIdsAsync(ids, withTracking: false, filterPredicate: UserFilter(user), ct: ct);

        return projects.ToPopulatedDtos();
    }

    public async Task<List<PopulatedProjectDto>> GetAllPopulatedAsync(UserOutgoingDto user, CancellationToken ct = default)
    {
        var projects = await _projectRepository.GetAllAsync(withTracking: false, filterPredicate: UserFilter(user), ct: ct);
        return projects.ToPopulatedDtos();
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
        var diagram = new InfluenceDiagramDto
        {
            projectId = projectId,
            issues = issueEntities.ToOutgoingDtos(),
            edges = edgeEntities.ToOutgoingDtos()
        };

        _cache.AddCacheItem(new CacheItem { CacheKey = CacheKeys.GetInfluenceDiagramKey(projectId) }, CacheConstants.DefaultMediumQueryCacheInTimeSpan, diagram);
        return diagram;
    }

    private static Expression<Func<Project, bool>> UserFilter(UserOutgoingDto user)
        => e => e.Public || e.ProjectRoles.Any(p => p.UserId == user.Id);

    private static Expression<Func<Issue, bool>> IssuesUserFilter(UserOutgoingDto user)
        => e => e.Project!.Public || e.Project!.ProjectRoles.Any(p => p.UserId == user.Id);

    private static Expression<Func<Edge, bool>> EdgesUserFilter(UserOutgoingDto user)
        => e => (e.HeadNode!.Issue!.Project!.Public || e.HeadNode!.Issue!.Project!.ProjectRoles.Any(p => p.UserId == user.Id)) && (e.TailNode!.Issue!.Project!.Public || e.TailNode!.Issue!.Project!.ProjectRoles.Any(p => p.UserId == user.Id));

    private void RegisterPublicProjectsInCache(List<ProjectOutgoingDto> projects)
    {
        var publicProjectIds = projects.Where(p => p.Public).Select(p => p.Id);
        if (publicProjectIds.Any())
        {
            var existing = _cache.GetCacheItem<HashSet<Guid>>(CacheKeys.PublicProjectIdsKey) ?? new HashSet<Guid>();
            var added = false;
            foreach (var id in publicProjectIds)
            {
                if (!existing.Contains(id))
                {
                    existing.Add(id);
                    added = true;
                }
            }
            if (added)
                _cache.AddCacheItem(new CacheItem { CacheKey = CacheKeys.PublicProjectIdsKey }, null, existing);
            var publicIds = _cache.GetPublicProjectIds();

        }
    }
}
