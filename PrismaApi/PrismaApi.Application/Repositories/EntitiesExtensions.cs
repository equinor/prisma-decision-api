using Microsoft.EntityFrameworkCore;
using PrismaApi.Domain.Dtos;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure;
using System.Threading.Tasks;

namespace PrismaApi.Application.Repositories;

public static class EntitiesExtensions
{
    public static void Update(this ICollection<ProjectRole> entities, ICollection<ProjectRole> incommingEntities, AppDbContext context)
    {
        // Update
        foreach (var entity in entities)
        {
            var inncommingEntity = incommingEntities.Where(x => x.Id == entity.Id).FirstOrDefault();
            if (inncommingEntity == null) continue;

            entity.ProjectId = inncommingEntity.ProjectId;
            entity.UserId = inncommingEntity.UserId;
            entity.Role = inncommingEntity.Role;
            entity.UpdatedById = inncommingEntity.UpdatedById;
        }

        // delete
        RepositoryUtilities.RemoveMissingFromCollectionMutate<ProjectRole, Guid>(incommingEntities, entities);

        // create
        RepositoryUtilities.AddMissingFromCollectionMutate<ProjectRole, Guid>(incommingEntities, entities, context);
    }

    public static void Update(this ICollection<Objective> entities, ICollection<Objective> incommingEntities, AppDbContext context)
    {
        // delete
        RepositoryUtilities.RemoveMissingFromCollectionMutate<Objective, Guid>(incommingEntities, entities);

        // create
        RepositoryUtilities.AddMissingFromCollectionMutate<Objective, Guid>(incommingEntities, entities, context);

        // update
        foreach (var entity in entities)
        {
            var incommingEntity = incommingEntities.Where(x => x.Id == entity.Id).First();

            entity.Name = incommingEntity.Name;
            entity.Type = incommingEntity.Type;
            entity.ProjectId = incommingEntity.ProjectId;
            entity.Description = incommingEntity.Description;
            entity.UpdatedById = incommingEntity.UpdatedById;
        }
    }

    public static void Update(this ICollection<Strategy> entities, ICollection<Strategy> incommingEntities, AppDbContext context)
    {
        // delete
        RepositoryUtilities.RemoveMissingFromCollectionMutate<Strategy, Guid>(incommingEntities, entities);

        // create
        RepositoryUtilities.AddMissingFromCollectionMutate<Strategy, Guid>(incommingEntities, entities, context);

        // update
        foreach (var entity in entities)
        {
            var incommingEntity = incommingEntities.Where(x => x.Id == entity.Id).First();


            entity.ProjectId = incommingEntity.ProjectId;
            entity.Description = incommingEntity.Description;
            entity.UpdatedById = incommingEntity.UpdatedById;
            entity.StrategyOptions.Update(incommingEntity.StrategyOptions, context);
        }
    }

    public static void Update(this ICollection<StrategyOption> entities, ICollection<StrategyOption> incommingEntities, AppDbContext context)
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
            context.Entry(entityToAdd).State = EntityState.Added;
            entities.Add(entityToAdd);
        }
    }

    public static Node Update(this Node entity, Node incommingEntity, AppDbContext context)
    {
        entity.Name = incommingEntity.Name;
        //entity.HeadEdges.Update(incommingEntity.HeadEdges, context);
        //entity.TailEdges.Update(incommingEntity.TailEdges, context);
        if (incommingEntity.NodeStyle != null && entity.NodeStyle != null)
            entity.NodeStyle = entity.NodeStyle.Update(incommingEntity.NodeStyle);
        return entity;
    }

    public static void Update(this ICollection<Edge> entities, ICollection<Edge> incommingEntities, AppDbContext context)
    {
        RepositoryUtilities.RemoveMissingFromCollectionMutate<Edge, Guid>(incommingEntities, entities);
        RepositoryUtilities.AddMissingFromCollectionMutate<Edge, Guid>(incommingEntities, entities, context);
        foreach (var entity in entities)
        {
            var inncommingEntity = incommingEntities.Where(x => x.Id == entity.Id).FirstOrDefault();
            if (inncommingEntity == null) continue;
            entity.HeadId = inncommingEntity.HeadId;
            entity.TailId = inncommingEntity.TailId;
        }
    }

    public static NodeStyle Update(this NodeStyle entity, NodeStyle incommingEntity)
    {
        entity.XPosition = incommingEntity.XPosition;
        entity.YPosition = incommingEntity.YPosition;
        return entity;
    }

    public static async Task<Decision> Update(this Decision entity, Decision incommingEntity, AppDbContext context, IDiscreteTableRuleTrigger? ruleTrigger = null)
    {
        if (entity.Type != incommingEntity.Type && incommingEntity.Type != "Foucus" && ruleTrigger != null)
            await ruleTrigger.ParentIssuesChangedAsync([entity.IssueId]);
        entity.Type = incommingEntity.Type;
        await entity.Options.Update(incommingEntity.Options, context, ruleTrigger);
        return entity;
    }

    public static async Task Update(this ICollection<Option> entities, ICollection<Option> incommingEntities, AppDbContext context, IDiscreteTableRuleTrigger? ruleTrigger = null)
    {
        //var entitiesToDelete = RepositoryUtilities.GetEntitiesToBeDeleted<Option, Guid>(incommingEntities, entities);
        //foreach (var entityToDelete in entitiesToDelete)
        //{
        //    context.Options.Remove(entityToDelete);
        //    entities.Remove(entityToDelete);
        //}
        RepositoryUtilities.AddMissingFromCollectionMutate<Option, Guid>(incommingEntities, entities, context);
        var entitiesToAdd = RepositoryUtilities.GetEntitiesToBeAdded<Option, Guid>(incommingEntities, entities);
        foreach (var entityToAdd in entitiesToAdd)
        {
            context.Entry(entityToAdd).State = EntityState.Added;
            entities.Add(entityToAdd);
        }
        if (ruleTrigger != null)
            await ruleTrigger.ParentOptionsAddedAsync([.. entitiesToAdd.Select(e => e.DecisionId)]);

        foreach (var entity in entities)
        {
            var inncommingEntity = incommingEntities.Where(x => x.Id == entity.Id).FirstOrDefault();
            if (inncommingEntity == null) continue;
            entity.Name = inncommingEntity.Name;
            entity.Utility = inncommingEntity.Utility;
        }
    }

    public static async Task<Uncertainty> Update(this Uncertainty entity, Uncertainty incommingEntity, AppDbContext context, IDiscreteTableRuleTrigger? ruleTrigger = null)
    {
        if (entity.IsKey != incommingEntity.IsKey && ruleTrigger != null)
            await ruleTrigger.ParentIssuesChangedAsync([entity.IssueId]);
            
        await entity.Outcomes.Update(incommingEntity.Outcomes, context, ruleTrigger);
        entity.IsKey = incommingEntity.IsKey;
        entity.DiscreteProbabilities.Update(incommingEntity.DiscreteProbabilities, context);
        return entity;
    }

    public static async Task Update(this ICollection<Outcome> entities, ICollection<Outcome> incommingEntities, AppDbContext context, IDiscreteTableRuleTrigger? ruleTrigger = null)
    {
        RepositoryUtilities.RemoveMissingFromCollectionMutate<Outcome, Guid>(incommingEntities, entities);
        //var entitiesToDelete = RepositoryUtilities.GetEntitiesToBeDeleted<Outcome, Guid>(incommingEntities, entities);
        //foreach (var entityToDelete in entitiesToDelete)
        //{
        //    context.Outcomes.Remove(entityToDelete);
        //    entities.Remove(entityToDelete);
        //}
        var entitiesToAdd = RepositoryUtilities.GetEntitiesToBeAdded<Outcome, Guid>(incommingEntities, entities);
        foreach (var entityToAdd in entitiesToAdd)
        {
            context.Entry(entityToAdd).State = EntityState.Added;
            entities.Add(entityToAdd);
        }
        if (ruleTrigger != null)
            await ruleTrigger.ParentOutcomesAddedAsync([.. entitiesToAdd.Select(e => e.UncertaintyId)]);

        foreach (var entity in entities)
        {
            var inncommingEntity = incommingEntities.Where(x => x.Id == entity.Id).FirstOrDefault();
            if (inncommingEntity == null) continue;
            entity.Name = inncommingEntity.Name;
            entity.Utility = inncommingEntity.Utility;
        }
    }

    public static void Update(this ICollection<DiscreteProbability> entities, ICollection<DiscreteProbability> incommingEntities, AppDbContext context)
    {
        RepositoryUtilities.RemoveMissingFromCollectionMutate<DiscreteProbability, Guid>(incommingEntities, entities);
        RepositoryUtilities.AddMissingFromCollectionMutate<DiscreteProbability, Guid>(incommingEntities, entities, context);
        foreach (var entity in entities)
        {
            var inncommingEntity = incommingEntities.Where(x => x.Id == entity.Id).FirstOrDefault();
            if (inncommingEntity == null) continue;
            entity.Probability = inncommingEntity.Probability;
        }
    }

    public static Utility Update(this Utility entity, Utility incommingEntity, AppDbContext context)
    {
        entity.DiscreteUtilities.Update(incommingEntity.DiscreteUtilities, context);
        return entity;
    }

    public static void Update(this ICollection<DiscreteUtility> entities, ICollection<DiscreteUtility> incommingEntities, AppDbContext context)
    {
        RepositoryUtilities.RemoveMissingFromCollectionMutate<DiscreteUtility, Guid>(incommingEntities, entities);
        RepositoryUtilities.AddMissingFromCollectionMutate<DiscreteUtility, Guid>(incommingEntities, entities, context);
        foreach (var entity in entities)
        {
            var inncommingEntity = incommingEntities.Where(x => x.Id == entity.Id).FirstOrDefault();
            if (inncommingEntity == null) continue;
            entity.UtilityValue = inncommingEntity.UtilityValue;
        }
    }
}
