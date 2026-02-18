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

            entity.ProjectRoles = UpdateProjectRolesViaProject(incommingEntity.ProjectRoles, entity.ProjectRoles);
            entity.Objectives = UpdateObjectivesViaProject(incommingEntity.Objectives, entity.Objectives);
            entity.Strategies = UpdateStrategiesViaProject(incommingEntity.Strategies, entity.Strategies);
        }

        await DbContext.SaveChangesAsync();
        return incommingEntities;
    }

    private ICollection<ProjectRole> UpdateProjectRolesViaProject(ICollection<ProjectRole> incommingEntities, ICollection<ProjectRole> entities)
    {
        // delete
        var entitiesToRemove = entities.Where(e => !incommingEntities.Any(ie => ie.Id == e.Id)).ToList();
        foreach (var entityToRemove in entitiesToRemove)
        {
            entities.Remove(entityToRemove);
        }

        // create
        var entitiesToAdd = incommingEntities.Where(ie => !entities.Any(e => e.Id == ie.Id)).ToList();
        foreach (var entityToAdd in entitiesToAdd)
        {
            entities.Add(entityToAdd);
        }

        // Update
        foreach (var entity in entities)
        {
            var inncommingEntity = incommingEntities.Where(x => x.Id == entity.Id).FirstOrDefault();
            if (inncommingEntity == null) continue;

            entity.ProjectId = inncommingEntity.ProjectId;
            entity.UserId = inncommingEntity.UserId;
            entity.Role = inncommingEntity.Role;
        }
        return entities;
    }

    private ICollection<Objective> UpdateObjectivesViaProject(ICollection<Objective> incommingEntities,  ICollection<Objective> entities)
    {
        // delete
        entities = RepositoryUtilities.RemoveMissingFromCollection<Objective, Guid>(incommingEntities, entities);

        // create
        entities = RepositoryUtilities.AddMissingFromCollection<Objective, Guid>(incommingEntities, entities);

        // update
        foreach (var entity in entities)
        {
            var incommingEntity = incommingEntities.Where(x => x.Id == entity.Id).First();

            entity.Name = incommingEntity.Name;
            entity.Type = incommingEntity.Type;
            entity.ProjectId = incommingEntity.ProjectId;
            entity.Description = incommingEntity.Description;
        }
        return entities;
    }

    private ICollection<Strategy> UpdateStrategiesViaProject(ICollection<Strategy> incommingEntities, ICollection<Strategy> entities)
    {
        // delete
        entities = RepositoryUtilities.RemoveMissingFromCollection<Strategy, Guid>(incommingEntities, entities);

        // create
        entities = RepositoryUtilities.AddMissingFromCollection<Strategy, Guid>(incommingEntities, entities);

        // update
        foreach (var entity in entities)
        {
            var incommingEntity = incommingEntities.Where(x => x.Id == entity.Id).First();


            entity.ProjectId = incommingEntity.ProjectId;
            entity.Description = incommingEntity.Description;
            entity.StrategyOptions = UpdateStrategyOptionsViaProject(incommingEntity.StrategyOptions, entity.StrategyOptions);
        }
        return entities;
    }

    private ICollection<StrategyOption> UpdateStrategyOptionsViaProject(ICollection<StrategyOption> incommingEntities, ICollection<StrategyOption> entities)
    {
        // delete
        var entitiesToRemove = entities.Where(e => !incommingEntities.Any(ie => ie.OptionId == e.OptionId && ie.StrategyId == e.StrategyId)).ToList();
        foreach (var entityToRemove in entitiesToRemove)
        {
            entities.Remove(entityToRemove);
        }

        // create
        var entitiesToAdd = incommingEntities.Where(ie => !entities.Any(e => e.OptionId == ie.OptionId && e.StrategyId == ie.StrategyId)).ToList();
        foreach (var entityToAdd in entitiesToAdd)
        {
            entities.Add(entityToAdd);
        }

        return entities;
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
