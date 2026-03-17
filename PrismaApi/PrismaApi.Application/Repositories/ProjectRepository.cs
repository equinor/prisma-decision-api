using Microsoft.EntityFrameworkCore;
using PrismaApi.Application.Interfaces.Repositories;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure.Context;
using System.Linq.Expressions;

namespace PrismaApi.Application.Repositories;

public class ProjectRepository : BaseRepository<Project, Guid>, IProjectRepository
{
    public readonly IProjectRoleRepository _repo;
    public ProjectRepository(AppDbContext dbContext, IProjectRoleRepository repo) : base(dbContext)
    {
        _repo = repo;
    }

    public async Task<ICollection<Project>> GetProjectsWhereUserHasAccess(ICollection<Guid> projectIds, int userId)
    {
        return await GetByIdsAsync(
            projectIds,
            withTracking: false,
            filterPredicate: p => p.ProjectRoles.Any(r => r.UserId == userId)
        );
    }


    public async Task<IEnumerable<Project>> UpdateRangeAsync(IEnumerable<Project> incommingEntities, Expression<Func<Project, bool>> filterPredicate)
    {
        var entities = await GetByIdsAsync(incommingEntities.Select(e => e.Id), withTracking: true, filterPredicate: filterPredicate);
        foreach (var entity in entities)
        {
            var incommingEntity = incommingEntities.Where(x => x.Id == entity.Id).First();

            entity.Name = incommingEntity.Name;
            entity.OpportunityStatement = incommingEntity.OpportunityStatement;
            entity.Public = incommingEntity.Public;
            entity.ParentProjectId = incommingEntity.ParentProjectId;
            entity.ParentProjectName = incommingEntity.ParentProjectName;
            entity.EndDate = incommingEntity.EndDate;
            entity.UpdatedById = incommingEntity.UpdatedById;

            entity.ProjectRoles.Update(incommingEntity.ProjectRoles, DbContext);
            entity.Objectives.Update(incommingEntity.Objectives, DbContext);
            entity.Strategies.Update(incommingEntity.Strategies, DbContext);
        }

        await DbContext.SaveChangesAsync();
        return incommingEntities;
    }

    protected override IQueryable<Project> Query()
    {
        return DbContext.Projects
            .Include(p => p.Objectives)
            .Include(p => p.ProjectRoles)
                .ThenInclude(pr => pr.User)
            .Include(p => p.Strategies)
                .ThenInclude(s => s.StrategyOptions)
                .ThenInclude(so => so.Option);
    }

    
}
