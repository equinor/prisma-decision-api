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

    public async Task<ICollection<Project>> GetProjectsWhereUserHasAccess(ICollection<Guid> projectIds, string userId)
    {
        return await GetByIdsAsync(
            projectIds,
            withTracking: false,
            filterPredicate: p => p.ProjectRoles.Any(r => r.UserId == userId)
        );
    }


    public async Task<IEnumerable<Project>> UpdateRangeAsync(IEnumerable<Project> incomingEntities, Expression<Func<Project, bool>> filterPredicate)
    {
        var entities = await GetByIdsAsync(incomingEntities.Select(e => e.Id), withTracking: true, filterPredicate: filterPredicate);
        foreach (var entity in entities)
        {
            var incomingEntity = incomingEntities.Where(x => x.Id == entity.Id).First();

            entity.Name = incomingEntity.Name;
            entity.OpportunityStatement = incomingEntity.OpportunityStatement;
            entity.Public = incomingEntity.Public;
            entity.ParentProjectId = incomingEntity.ParentProjectId;
            entity.ParentProjectName = incomingEntity.ParentProjectName;
            entity.EndDate = incomingEntity.EndDate;
            entity.UpdatedById = incomingEntity.UpdatedById;

            entity.ProjectRoles.Update(incomingEntity.ProjectRoles, DbContext);
            entity.Objectives.Update(incomingEntity.Objectives, DbContext);
            entity.Strategies.Update(incomingEntity.Strategies, DbContext);
        }

        await DbContext.SaveChangesAsync();
        return incomingEntities;
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
