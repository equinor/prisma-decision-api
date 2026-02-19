using Microsoft.EntityFrameworkCore;
using Microsoft.Graph.Me.InferenceClassification.Overrides.Item;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure;
using System.Linq;
using System.Security.Cryptography;

namespace PrismaApi.Application.Repositories;

public class ProjectRepository : BaseRepository<Project, Guid>
{
    public ProjectRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public override async Task<IEnumerable<Project>> UpdateRangeAsync(IEnumerable<Project> incommingEntities)
    {
        var entities = await GetByIdsAsync(incommingEntities.Select(e => e.Id));
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

            entity.ProjectRoles = entity.ProjectRoles.Update(incommingEntity.ProjectRoles);
            entity.Objectives = entity.Objectives.Update(incommingEntity.Objectives);
            entity.Strategies = entity.Strategies.Update(incommingEntity.Strategies);
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
