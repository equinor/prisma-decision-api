using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Application.Mapping;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;
using System.Linq.Expressions;

namespace PrismaApi.Application.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectRoleRepository _projectRoleRepository;
    private readonly IIssueRepository _issueRepository;
    private readonly IEdgeRepository _edgeRepository;
    private readonly IUserRepository _userRepository;

    public ProjectService(
        IProjectRepository projectRepository,
        IProjectRoleRepository projectRoleRepository,
        IIssueRepository issueRepository,
        IEdgeRepository edgeRepository,
        IUserRepository userRepository)
    {
        _projectRepository = projectRepository;
        _projectRoleRepository = projectRoleRepository;
        _issueRepository = issueRepository;
        _edgeRepository = edgeRepository;
        _userRepository = userRepository;
    }

    public async Task<List<ProjectOutgoingDto>> CreateAsync(List<ProjectCreateDto> dtos, bool isProjectDuplicated, UserOutgoingDto userDto)
    {
        var projectEntities = dtos.ToEntities(userDto);

        await _projectRepository.AddRangeAsync(projectEntities);

        if (isProjectDuplicated)
        {

            var creatorRole = dtos.Select(x =>
            {
                return new ProjectRoleCreateDto
                {
                    Id = Guid.NewGuid(),
                    ProjectId = x.Id,
                    UserId = userDto.Id,
                    Role = ProjectRoleType.Facilitator.ToString()
                };
            }).Distinct();

            await _projectRoleRepository.AddRangeAsync(creatorRole.ToEntities(userDto));
        }

        var ids = projectEntities.Select(p => p.Id).ToList();
        var projects = await _projectRepository.GetByIdsAsync(ids, withTracking: false);
        return projects.ToOutgoingDtos();
    }

    public async Task<List<ProjectOutgoingDto>> UpdateAsync(List<ProjectIncomingDto> dtos, UserOutgoingDto userDto)
    {
        var projectEntities = dtos.ToEntities(userDto);
        await _projectRepository.UpdateRangeAsync(projectEntities, filterPredicate: UserFilter(userDto));

        foreach (var dto in dtos)
        {
            if (dto.Users.Count == 0)
            {
                continue;
            }

            var projectRoles = await BuildProjectRolesAsync(dto.Users, dto.Id, userDto);
            await _projectRoleRepository.UpdateRangeAsync(projectRoles);
        }

        var ids = dtos.Select(d => d.Id).ToList();
        var projects = await _projectRepository.GetByIdsAsync(ids, withTracking: false, filterPredicate: UserFilter(userDto));
        return projects.ToOutgoingDtos();
    }

    public async Task DeleteAsync(List<Guid> ids, UserOutgoingDto user)
    {
        await _projectRepository.DeleteByIdsAsync(ids, filterPredicate: UserFilter(user));
    }

    public async Task<List<ProjectOutgoingDto>> GetAsync(List<Guid> ids, UserOutgoingDto user)
    {
        var projects = await _projectRepository.GetByIdsAsync(ids, withTracking: false, filterPredicate: UserFilter(user));
        return projects.ToOutgoingDtos();
    }

    public async Task<List<ProjectOutgoingDto>> GetAllAsync(UserOutgoingDto user)
    {
        var projects = await _projectRepository.GetAllAsync(withTracking: false, filterPredicate: UserFilter(user));
        return projects.ToOutgoingDtos();
    }

    public async Task<List<PopulatedProjectDto>> GetPopulatedAsync(List<Guid> ids, UserOutgoingDto user)
    {
        var projects = await _projectRepository.GetByIdsAsync(ids, withTracking: false, filterPredicate: UserFilter(user));
        return projects.ToPopulatedDtos();
    }

    public async Task<List<PopulatedProjectDto>> GetAllPopulatedAsync(UserOutgoingDto user)
    {
        var projects = await _projectRepository.GetAllAsync(withTracking: false, filterPredicate: UserFilter(user));
        return projects.ToPopulatedDtos();
    }

    public async Task<InfluanceDiagramDto> GetInfluanceDiagramAsync(Guid projectId, UserOutgoingDto user)
    {
        var project = await _projectRepository.GetByIdsAsync([projectId], withTracking: false, filterPredicate: UserFilter(user));

        if (project == null)
            throw new ArgumentNullException(nameof(project));

        return new InfluanceDiagramDto
        {
            projectId = projectId,
            issues = (await _issueRepository.GetIssuesInInfluenceDiagram(projectId, IssuesUserFilter(user))).ToOutgoingDtos(),
            edges = (await _edgeRepository.GetEdgesInInfluenceDiagram(projectId, EdgesUserFilter(user))).ToOutgoingDtos()
        };
    }

    private async Task<List<ProjectRole>> BuildProjectRolesAsync(
        IEnumerable<ProjectRoleIncomingDto> dtos,
        Guid projectId,
        UserOutgoingDto userDto)
    {
        var result = new List<ProjectRole>();

        foreach (var dto in dtos)
        {
            var user = await _userRepository.GetByAzureIdAsync(dto.AzureId);
            if (user == null)
            {
                user = new User
                {
                    Name = dto.Name,
                    AzureId = dto.AzureId
                };

                await _userRepository.AddAsync(user);
            }

            result.Add(new ProjectRole
            {
                Id = dto.Id,
                ProjectId = projectId,
                UserId = user.Id,
                Role = dto.Role,
                User = user,
                CreatedById = userDto.Id,
                UpdatedById = userDto.Id
            });
        }

        return result;
    }

    private async Task<List<ProjectRole>> BuildProjectRolesAsync(
        IEnumerable<ProjectRoleCreateDto> dtos,
        Guid projectId,
        UserOutgoingDto userDto)
    {
        var result = new List<ProjectRole>();

        foreach (var dto in dtos)
        {
            var user = await _userRepository.GetByAzureIdAsync(dto.AzureId);
            if (user == null)
            {
                user = new User
                {
                    Name = dto.Name,
                    AzureId = dto.AzureId
                };

                await _userRepository.AddAsync(user);
            }

            result.Add(new ProjectRole
            {
                Id = dto.Id,
                ProjectId = projectId,
                UserId = user.Id,
                Role = dto.Role,
                User = user,
                CreatedById = userDto.Id,
                UpdatedById = userDto.Id
            });
        }

        return result;
    }

    private static Expression<Func<Project, bool>> UserFilter(UserOutgoingDto user)
        => e => e.ProjectRoles.Any(p => p.UserId == user.Id);

    private static Expression<Func<Issue, bool>> IssuesUserFilter(UserOutgoingDto user)
        => e => e.Project!.ProjectRoles.Any(p => p.UserId == user.Id);

    private static Expression<Func<Edge, bool>> EdgesUserFilter(UserOutgoingDto user)
        => e => e.HeadNode!.Issue!.Project!.ProjectRoles.Any(p => p.UserId == user.Id) && e.TailNode!.Issue!.Project!.ProjectRoles.Any(p => p.UserId == user.Id);
}
