using Microsoft.EntityFrameworkCore;
using Microsoft.Graph.Me.InferenceClassification.Overrides.Item;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace PrismaApi.Application.Repositories;

public class ProjectRepository : BaseRepository<Project, Guid>
{
    public readonly ProjectRoleRepository _repo;
    public ProjectRepository(AppDbContext dbContext, ProjectRoleRepository repo) : base(dbContext)
    {
        _repo = repo;
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
            //await UpdateProjectRoles(entity, incommingEntity);
            entity.Objectives = entity.Objectives.Update(incommingEntity.Objectives);
            entity.Strategies = entity.Strategies.Update(incommingEntity.Strategies);
            //await DbContext.SaveChangesAsync();
        }

        try
        {
            await DbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            // Reload entities from database to get latest values and retry
            throw e;
        }
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
