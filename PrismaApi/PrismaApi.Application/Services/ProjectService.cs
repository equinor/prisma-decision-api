using PrismaApi.Application.Interfaces;
using PrismaApi.Application.Mapping;
using PrismaApi.Application.Repositories;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;

namespace PrismaApi.Application.Services;

public class ProjectService : IProjectService
{
    private readonly ProjectRepository _projectRepository;
    private readonly ProjectRoleRepository _projectRoleRepository;
    private readonly IssueRepository _issueRepository;
    private readonly EdgeRepository _edgeRepository;
    private readonly UserRepository _userRepository;

    public ProjectService(
        ProjectRepository projectRepository,
        ProjectRoleRepository projectRoleRepository,
        IssueRepository issueRepository,
        EdgeRepository edgeRepository,
        UserRepository userRepository)
    {
        _projectRepository = projectRepository;
        _projectRoleRepository = projectRoleRepository;
        _issueRepository = issueRepository;
        _edgeRepository = edgeRepository;
        _userRepository = userRepository;
    }

    public async Task<List<ProjectOutgoingDto>> CreateAsync(List<ProjectCreateDto> dtos, UserOutgoingDto userDto)
    {
        var rolesToBeCreated = dtos.Select(x =>
        {
            return new ProjectRoleCreateDto
            {
                Id = Guid.NewGuid(),
                ProjectId = x.Id,
                UserId = userDto.Id,
                Role = ProjectRoleType.Facilitator.ToString()
            };
        }).Distinct();

        foreach (var dto in dtos)
        {
            var creatorRole = new ProjectRoleCreateDto
            {
                Id = Guid.NewGuid(),
                ProjectId = dto.Id,
                UserId = userDto.Id,
                Role = ProjectRoleType.Facilitator.ToString()
            };
        }

        var projectEntities = dtos.ToEntities(userDto);
        await _projectRepository.AddRangeAsync(projectEntities);

        //foreach (var dto in dtos)
        //{
        //    if (dto.Users.Count == 0)
        //    {
        //        continue;
        //    }

        //    var projectRoles = await BuildProjectRolesAsync(dto.Users, dto.Id, userDto);
        //    await _projectRoleRepository.AddRangeAsync(projectRoles);
        //}

        await _projectRoleRepository.AddRangeAsync(rolesToBeCreated.ToEntities(userDto));

        var ids = projectEntities.Select(p => p.Id).ToList();
        var projects = await _projectRepository.GetByIdsAsync(ids, withTracking: false);
        return projects.ToOutgoingDtos();
    }

    public async Task<List<ProjectOutgoingDto>> UpdateAsync(List<ProjectIncomingDto> dtos, UserOutgoingDto userDto)
    {
        var projectEntities = dtos.ToEntities(userDto);
        await _projectRepository.UpdateRangeAsync(projectEntities);

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
        var projects = await _projectRepository.GetByIdsAsync(ids, withTracking: false);
        return projects.ToOutgoingDtos();
    }

    public async Task DeleteAsync(List<Guid> ids)
    {
        await _projectRepository.DeleteByIdsAsync(ids);
    }

    public async Task<List<ProjectOutgoingDto>> GetAsync(List<Guid> ids)
    {
        var projects = await _projectRepository.GetByIdsAsync(ids, withTracking: false);
        return projects.ToOutgoingDtos();
    }

    public async Task<List<ProjectOutgoingDto>> GetAllAsync(UserOutgoingDto user)
    {
        var projects = await _projectRepository.GetAllAsyncProjects(user, withTracking: false);
        return projects.ToOutgoingDtos();
    }

    public async Task<List<PopulatedProjectDto>> GetPopulatedAsync(List<Guid> ids)
    {
        var projects = await _projectRepository.GetByIdsAsync(ids, withTracking: false);
        return projects.ToPopulatedDtos();
    }

    public async Task<List<PopulatedProjectDto>> GetAllPopulatedAsync()
    {
        var projects = await _projectRepository.GetAllAsync(withTracking: false);
        return projects.ToPopulatedDtos();
    }

    public async Task<InfluanceDiagramDto> GetInfluanceDiagramAsync(Guid projectId)
    {
        return new InfluanceDiagramDto
        {
            projectId = projectId,
            issues = (await _issueRepository.GetIssuesInInfluenceDiagram(projectId)).ToOutgoingDtos(),
            edges = (await _edgeRepository.GetEdgesInInfluenceDiagram(projectId)).ToOutgoingDtos()
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
                    Name = dto.UserName,
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
                    Name = dto.UserName,
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
}
