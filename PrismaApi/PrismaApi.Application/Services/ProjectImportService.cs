using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Services;

/// <summary>Service for importing projects from JSON payloads using duplication logic.</summary>
public class ProjectImportService : IProjectImportService
{
    private readonly IProjectDuplicationService _projectDuplicationService;

    public ProjectImportService(IProjectDuplicationService projectDuplicationService)
    {
        _projectDuplicationService = projectDuplicationService;
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
