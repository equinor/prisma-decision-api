using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Services;

/// <summary>Service for importing projects from JSON payloads using duplication logic.</summary>
public class ProjectImportService : IProjectImportService
{
    private readonly IProjectDuplicationService _projectDuplicationService;
    private readonly IUserRepository _userRepository;

    public ProjectImportService(IProjectDuplicationService projectDuplicationService, IUserRepository userRepository)
    {
        _projectDuplicationService = projectDuplicationService;
        _userRepository = userRepository;
    }

    public async Task<List<ProjectOutgoingDto>> ImportFromJsonWithDuplicationLogicAsync(
        List<ProjectImportDto> dtos,
        UserOutgoingDto user,
        CancellationToken cancellationToken = default)
    {
        if (dtos == null || dtos.Count == 0)
        {
            return new List<ProjectOutgoingDto>();
        }

        var createdProjects = new List<ProjectOutgoingDto>();

        foreach (var dto in dtos)
        {
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var userDto in dto.Projects.Users)
            {
                if (userDto != null && userDto.AzureId != null)
                    userDto.UserId = userDto.AzureId.Value.ToString();

                // make sure all users are in the database
                if (userDto != null)
                {
                    var userForRole = new UserIncomingDto { Id = userDto.UserId, Name = userDto.Name };
                    await _userRepository.GetOrAddByIdAsync(userForRole);
                }
            }

            var createdProject = await _projectDuplicationService.DuplicateImportedProjectAsync(dto, user, cancellationToken);
            if (createdProject is null)
            {
                return new List<ProjectOutgoingDto>();
            }

            createdProjects.Add(createdProject);
        }

        return createdProjects;
    }

}
