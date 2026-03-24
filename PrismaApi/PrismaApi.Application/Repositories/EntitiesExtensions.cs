using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PrismaApi.Domain.Constants;
using PrismaApi.Domain.Entities;
using PrismaApi.Infrastructure.Context;
using PrismaApi.Infrastructure.Interfaces;

namespace PrismaApi.Application.Repositories;

public static class EntitiesExtensions
{
    public static void Update(this ICollection<ProjectRole> entities, ICollection<ProjectRole> incommingEntities, AppDbContext context)
    {
        // delete
        RepositoryUtilities.RemoveMissingFromCollectionMutate<ProjectRole, Guid>(incommingEntities, entities);

        // create
        RepositoryUtilities.AddMissingFromCollectionMutate<ProjectRole, Guid>(incommingEntities, entities, context);

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
            entity.Name = incommingEntity.Name;
            entity.Description = incommingEntity.Description;
            entity.Rationale = incommingEntity.Rationale;
            entity.Icon = incommingEntity.Icon;
            entity.IconColor = incommingEntity.IconColor;
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

    public static Node Update(this Node entity, Node incommingEntity)
    {
        entity.ProjectId = incommingEntity.ProjectId;
        entity.IssueId = incommingEntity.IssueId;
        entity.Name = incommingEntity.Name;
        if (incommingEntity.NodeStyle != null && entity.NodeStyle != null)
            entity.NodeStyle = entity.NodeStyle.Update(incommingEntity.NodeStyle);
        return entity;
    }

    public static NodeStyle Update(this NodeStyle entity, NodeStyle incommingEntity)
    {
        entity.NodeId = incommingEntity.NodeId;
        entity.XPosition = incommingEntity.XPosition;
        entity.YPosition = incommingEntity.YPosition;
        return entity;
    }

    public static async Task<Decision> Update(this Decision entity, Decision incommingEntity, AppDbContext context, IDiscreteTableRuleEventHandler? ruleTrigger = null)
    {
        if (entity.Type != incommingEntity.Type && incommingEntity.Type != DecisionHierarchy.Focus.ToString() && ruleTrigger != null)
            await ruleTrigger.ParentIssuesChangedAsync([entity.IssueId]);
        await entity.RemoveOutOfScopeStrategyOptions(incommingEntity, context);
        entity.IssueId = incommingEntity.IssueId;
        entity.Type = incommingEntity.Type;
        await entity.Options.Update(incommingEntity.Options, context, ruleTrigger);
        return entity;
    }

    public static async Task Update(this ICollection<Option> entities, ICollection<Option> incommingEntities, AppDbContext context, IDiscreteTableRuleEventHandler? ruleTrigger = null)
    {
        RepositoryUtilities.RemoveMissingFromCollectionMutate<Option, Guid>(incommingEntities, entities);
        var entitiesToAdd = RepositoryUtilities.GetEntitiesToBeAdded<Option, Guid>(incommingEntities, entities);
        foreach (var entityToAdd in entitiesToAdd)
        {
            context.Entry(entityToAdd).State = EntityState.Added;
            entities.Add(entityToAdd);
        }
        if (ruleTrigger != null)
            await ruleTrigger.OnDecisionOptionsAddedAsync([.. entitiesToAdd.Select(e => e.DecisionId)]);

        foreach (var entity in entities)
        {
            var inncommingEntity = incommingEntities.Where(x => x.Id == entity.Id).FirstOrDefault();
            if (inncommingEntity == null) continue;
            entity.DecisionId = inncommingEntity.DecisionId;
            entity.Name = inncommingEntity.Name;
            entity.Utility = inncommingEntity.Utility;
        }
    }

    public static async Task<Uncertainty> Update(this Uncertainty entity, Uncertainty incommingEntity, AppDbContext context, IDiscreteTableRuleEventHandler? ruleTrigger = null)
    {
        if (entity.IsKey != incommingEntity.IsKey && ruleTrigger != null)
            await ruleTrigger.ParentIssuesChangedAsync([entity.IssueId]);
            
        await entity.Outcomes.Update(incommingEntity.Outcomes, context, ruleTrigger);
        entity.IssueId = incommingEntity.IssueId;
        entity.IsKey = incommingEntity.IsKey;
        entity.DiscreteProbabilities.Update(incommingEntity.DiscreteProbabilities, context);
        return entity;
    }

    public static async Task Update(this ICollection<Outcome> entities, ICollection<Outcome> incommingEntities, AppDbContext context, IDiscreteTableRuleEventHandler? ruleTrigger = null)
    {
        RepositoryUtilities.RemoveMissingFromCollectionMutate<Outcome, Guid>(incommingEntities, entities);
        var entitiesToAdd = RepositoryUtilities.GetEntitiesToBeAdded<Outcome, Guid>(incommingEntities, entities);
        foreach (var entityToAdd in entitiesToAdd)
        {
            context.Entry(entityToAdd).State = EntityState.Added;
            entities.Add(entityToAdd);
        }
        if (ruleTrigger != null)
            await ruleTrigger.OnUncertaintyOutcomesAddedAsync([.. entitiesToAdd.Select(e => e.UncertaintyId)]);

        foreach (var entity in entities)
        {
            var inncommingEntity = incommingEntities.Where(x => x.Id == entity.Id).FirstOrDefault();
            if (inncommingEntity == null) continue;
            entity.UncertaintyId = inncommingEntity.UncertaintyId;
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
        entity.IssueId = incommingEntity.IssueId;
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

  public static async Task RemoveOutOfScopeStrategyOptions(this Issue entity, Issue incommingEntity, AppDbContext context)
    {
        if (!RepositoryUtilities.IsDecisionMovedOutOfStrategyTable(entity, incommingEntity)) return;
        await RemoveStrategyOptions(context, e => e.Option!.Decision!.IssueId == entity.Id);
    }

    public static async Task RemoveOutOfScopeStrategyOptions(this Decision entity, Decision incommingEntity, AppDbContext context)
    {
        if (!RepositoryUtilities.IsDecisionMovedOutOfStrategyTable(entity, incommingEntity)) return;
        await RemoveStrategyOptions(context, e => e.Option!.DecisionId == entity.Id);
    }

    private static async Task RemoveStrategyOptions(AppDbContext context, Expression<Func<StrategyOption, bool>> predicate)
    {
        var strategyOptionsToBeRemoved = await context.StrategyOptions
            .Where(predicate)
            .ToListAsync();
        if (strategyOptionsToBeRemoved.Count != 0)
        {
            context.StrategyOptions.RemoveRange(strategyOptionsToBeRemoved);
            await context.SaveChangesAsync();
        }
    }
}
