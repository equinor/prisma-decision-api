using PrismaApi.Domain.Dtos;

namespace PrismaApi.Application.Interfaces.Services;

public interface IProjectImportService
{
    /// <summary>
    /// Import projects from JSON data using project duplication logic.
    /// </summary>
    Task<List<ProjectOutgoingDto>> ImportFromJsonWithDuplicationLogicAsync(
        List<ProjectImportDto> dtos,
        UserOutgoingDto user,
        CancellationToken cancellationToken = default
    );
}
