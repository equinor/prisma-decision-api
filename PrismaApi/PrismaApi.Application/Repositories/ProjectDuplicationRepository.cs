using Microsoft.EntityFrameworkCore;
using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Application.Mapping;
using PrismaApi.Domain.Dtos;
using PrismaApi.Infrastructure;

namespace PrismaApi.Application.Repositories;

public class ProjectDuplicationRepository : IProjectDuplicationRepository
{
    private readonly AppDbContext _dbContext;

    public ProjectDuplicationRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<FullProjectForDuplicationDto?> GetFullProjectForDuplicationAsync(
        Guid projectId,
        UserOutgoingDto user,
        CancellationToken ct = default)
    {
        var project = await _dbContext.Projects
            .AsNoTracking()
            .Where(p => p.Id == projectId && p.ProjectRoles.Any(pr => pr.UserId == user.Id))
            .Include(p => p.Objectives)
            .Include(p => p.ProjectRoles).ThenInclude(pr => pr.User)
            .Include(p => p.Strategies).ThenInclude(s => s.StrategyOptions).ThenInclude(so => so.Option)
            .Include(p => p.Edges)
            .Include(p => p.Issues).ThenInclude(i => i.Node).ThenInclude(n => n!.NodeStyle)
            .Include(p => p.Issues).ThenInclude(i => i.Decision).ThenInclude(d => d!.Options)
            .Include(p => p.Issues).ThenInclude(i => i.Uncertainty).ThenInclude(u => u!.Outcomes)
            .Include(p => p.Issues).ThenInclude(i => i.Uncertainty)
                .ThenInclude(u => u!.DiscreteProbabilities).ThenInclude(dp => dp.ParentOutcomes)
            .Include(p => p.Issues).ThenInclude(i => i.Uncertainty)
                .ThenInclude(u => u!.DiscreteProbabilities).ThenInclude(dp => dp.ParentOptions)
            .Include(p => p.Issues).ThenInclude(i => i.Utility)
                .ThenInclude(u => u!.DiscreteUtilities).ThenInclude(du => du.ParentOutcomes)
            .Include(p => p.Issues).ThenInclude(i => i.Utility)
                .ThenInclude(u => u!.DiscreteUtilities).ThenInclude(du => du.ParentOptions)
            .AsSplitQuery()
            .FirstOrDefaultAsync(ct);

        return project?.ToFullProjectForDuplicationDto();
    }
}
